using Confluent.Kafka;
using YsEdu.Core.Config;
using YsEdu.Core.Data;
using YsEdu.Core.Logging;
using YsEdu.Core.Messaging;

namespace YsEdu.Core.Services;

/// 화면에 보여줄 처리 1건
public sealed record ProcessedInfo(DateTime Time, string Topic, int Partition, long Offset, string MessageName, string EventType,
                                   string EqpId, string LotId, string Result, string Reason, bool Duplicate, string RequestXml, string ResponseXml);

/// <summary>
/// Kafka 연계 미들웨어 실행부.
///   Consumer  : YSEDU.LOT.REQUEST 수신 (자동 Commit 끔 — 처리와 응답이 끝난 메시지만 Commit)
///   처리      : MessageProcessor (DB)
///   Producer  : YSEDU.LOT.RESPONSE로 응답 XML 송신 (Key = EQPID)
/// 수신 대기는 별도 스레드(Task)에서 돌기 때문에 UI가 멈추지 않고,
/// 처리·Commit 오류가 나도 수신 루프는 멈추지 않는다 (2차 p.12 "이후 수신 지속").
/// Stop()은 수신 작업이 실제로 끝날 때까지 기다린 뒤 "수신 작업 종료 확인"을 기록한다.
/// </summary>
public sealed class MiddlewareWorker : IAsyncDisposable
{
    private readonly AppSettings _settings;
    private readonly FileLog _log;
    private readonly MessageProcessor _processor;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public MiddlewareWorker(AppSettings settings, FileLog log)
    {
        _settings = settings;
        _log = log;
        _processor = new MessageProcessor(new DbFactory(settings.ConnectionString), log);
    }

    /// 메시지 1건 처리가 끝날 때마다 발생 (수신 스레드에서 호출됨 → 화면은 BeginInvoke로 받을 것)
    public event Action<ProcessedInfo>? Processed;

    public bool IsRunning => _loop is { IsCompleted: false };

    public void Start()
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _loop = Task.Run(() => RunAsync(token), token);
        _log.Info($"[시작] 수신 작업 시작 — {_settings.BootstrapServers} {_settings.RequestTopic} (group {_settings.MiddlewareGroupId})");
    }

    public async Task StopAsync()
    {
        if (_cts == null || _loop == null) return;
        _cts.Cancel();
        try { await _loop; } catch (OperationCanceledException) { }
        _cts.Dispose();
        _cts = null;
        _log.Info("[중지] 수신 작업 종료 확인 — Consumer Close 완료");
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    // ▶ 수신 루프
    private async Task RunAsync(CancellationToken token)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.MiddlewareGroupId,
            EnableAutoCommit = false,                       // 처리 후 직접 Commit
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig)
            .SetErrorHandler((_, err) => _log.Warn($"[Kafka Consumer] {err.Code} {err.Reason}"))
            .Build();
        using var producer = new ProducerBuilder<string, string>(producerConfig)
            .SetErrorHandler((_, err) => _log.Warn($"[Kafka Producer] {err.Code} {err.Reason}"))
            .Build();

        consumer.Subscribe(_settings.RequestTopic);
        try
        {
            while (!token.IsCancellationRequested)
            {
                ConsumeResult<string, string>? cr;
                try
                {
                    cr = consumer.Consume(TimeSpan.FromMilliseconds(500));
                }
                catch (ConsumeException ex)
                {
                    _log.Error($"[수신 오류] {ex.Error.Reason}");
                    continue;
                }
                if (cr == null || cr.IsPartitionEOF) continue;

                try
                {
                    await HandleAsync(consumer, producer, cr, token);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // 예상하지 못한 처리 오류: 기록하고 Commit하지 않은 채 같은 위치를 다시 처리 (이후 수신 지속)
                    _log.Error($"[처리 오류] {cr.TopicPartitionOffset} {ex.GetBaseException().Message} — 다시 처리 예정");
                    consumer.Seek(cr.TopicPartitionOffset);
                    await Task.Delay(1000, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 중지 요청
        }
        finally
        {
            consumer.Close();                               // 그룹에서 정상 탈퇴
            producer.Flush(TimeSpan.FromSeconds(5));
        }
    }

    private async Task HandleAsync(IConsumer<string, string> consumer, IProducer<string, string> producer,
                                   ConsumeResult<string, string> cr, CancellationToken token)
    {
        var outcome = _processor.Process(cr.Message.Value, cr.Topic, cr.Partition.Value, cr.Offset.Value);
        var r = outcome.Response;

        try
        {
            // 응답 Key = EQPID (같은 설비 응답끼리 같은 Partition → 순서 유지)
            await producer.ProduceAsync(_settings.ResponseTopic,
                new Message<string, string> { Key = r.EqpId, Value = outcome.ResponseXml }, token);
            consumer.Commit(cr);                            // 처리 + 응답까지 끝난 것만 Commit
        }
        catch (ProduceException<string, string> ex)
        {
            // 응답 전송 실패: Commit하지 않고 같은 위치로 되돌려 다시 처리한다.
            // 다시 처리할 때는 ③ 중복 확인에 걸려 이미 반영한 결과로 응답만 다시 보낸다.
            _log.Error($"[응답 전송 실패] {r.MessageName}: {ex.Error.Reason} — {cr.TopicPartitionOffset} 재처리 예정");
            consumer.Seek(cr.TopicPartitionOffset);
            await Task.Delay(1000, token);
            return;
        }
        catch (KafkaException ex)
        {
            // Commit 실패: 응답은 이미 보냈으므로 기록만 하고 수신을 계속한다 (다시 받더라도 중복 확인에 걸림)
            _log.Error($"[Commit 실패] {r.MessageName}: {ex.Error.Reason} — {cr.TopicPartitionOffset}");
        }

        string text = $"{cr.TopicPartitionOffset} {r.MessageName} {r.EventType} {r.EqpId} {r.LotId} → {r.Result} {r.Reason} ({outcome.Detail})";
        if (r.Result == Results.ERROR) _log.Error(text);
        else if (outcome.Duplicate) _log.Warn(text);
        else _log.Info(text);

        Processed?.Invoke(new ProcessedInfo(DateTime.Now, cr.Topic, cr.Partition.Value, cr.Offset.Value, r.MessageName, r.EventType,
                                            r.EqpId, r.LotId, r.Result, r.Reason, outcome.Duplicate, cr.Message.Value, outcome.ResponseXml));
    }
}
