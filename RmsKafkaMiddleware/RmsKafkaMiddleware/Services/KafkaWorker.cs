using Confluent.Kafka;
using RmsKafkaMiddleware.Common;

namespace RmsKafkaMiddleware.Services;

/// 화면 표시용 처리 1건
public sealed record HandledMessage(DateTime Time, int Partition, long Offset, string ResultCode, string ResultMessage,
                                   string? EqpId, string? RecipeId, long? ResultId, string RequestXml, string ResponseXml);

/// <summary>
/// Kafka 수신·응답 루프
///   01 Kafka 요청 수신 : Consumer로 RMS.REQUEST 구독 (자동 Commit 끔)
///   02~05 처리        : RmsRequestProcessor
///   04 응답 XML 전송  : Producer로 RMS.RESPONSE에 송신
///   Commit            : 처리와 응답이 끝난 메시지만 Commit
/// Kafka 오류(응답 전송 실패)는 기록하고, Commit하지 않은 채 같은 위치를 다시 처리한다 (메시지 처리 상태 유지).
/// 다시 처리할 때는 중복 확인에 걸려 저장은 한 번만 된다.
/// </summary>
public sealed class KafkaWorker(AppSettings settings, FileLog log) : IAsyncDisposable
{
    private readonly RmsRequestProcessor _processor = new(settings, log);
    private CancellationTokenSource? _cts;
    private Task? _loop;

    /// 처리 1건이 끝날 때마다 발생 (수신 스레드)
    public event Action<HandledMessage>? Handled;

    public bool IsRunning => _loop is { IsCompleted: false };

    public void Start()
    {
        if (IsRunning) return;
        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _loop = Task.Run(() => RunAsync(token), token);
        log.Info($"[시작] {settings.BootstrapServers} {settings.RequestTopic} 수신 → {settings.ResponseTopic} 응답 (group {settings.GroupId})");
    }

    public async Task StopAsync()
    {
        if (_cts == null || _loop == null) return;
        _cts.Cancel();
        try { await _loop; } catch (OperationCanceledException) { }
        _cts.Dispose();
        _cts = null;
        log.Info("[중지] 수신 작업 종료 확인");
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    private async Task RunAsync(CancellationToken token)
    {
        using var consumer = new ConsumerBuilder<string?, string>(new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        })
        .SetErrorHandler((_, e) => log.Warn($"[Kafka Consumer] {e.Code} {e.Reason}"))
        .Build();

        using var producer = new ProducerBuilder<string?, string>(new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        })
        .SetErrorHandler((_, e) => log.Warn($"[Kafka Producer] {e.Code} {e.Reason}"))
        .Build();

        consumer.Subscribe(settings.RequestTopic);
        try
        {
            while (!token.IsCancellationRequested)
            {
                ConsumeResult<string?, string>? cr;
                try
                {
                    cr = consumer.Consume(TimeSpan.FromMilliseconds(500));       // 01 Kafka 요청 수신
                }
                catch (ConsumeException ex)
                {
                    log.Error($"[Kafka 수신 오류] {ex.Error.Reason}");
                    continue;
                }
                if (cr == null || cr.IsPartitionEOF) continue;

                var outcome = _processor.Process(cr.Message.Value ?? "", cr.Topic, cr.Partition.Value, cr.Offset.Value);

                try
                {
                    // 04 응답 XML 전송 (Key = EQP_ID)
                    await producer.ProduceAsync(settings.ResponseTopic,
                        new Message<string?, string> { Key = outcome.EqpId, Value = outcome.ResponseXml }, token);
                    consumer.Commit(cr);
                }
                catch (ProduceException<string?, string> ex)
                {
                    log.Error($"[Kafka 응답 전송 실패] {ex.Error.Reason} — {cr.TopicPartitionOffset} 다시 처리 예정");
                    consumer.Seek(cr.TopicPartitionOffset);
                    await Task.Delay(1000, token);
                    continue;
                }

                Handled?.Invoke(new HandledMessage(DateTime.Now, cr.Partition.Value, cr.Offset.Value, outcome.ResultCode, outcome.ResultMessage,
                                                   outcome.EqpId, outcome.RecipeId, outcome.ResultId, cr.Message.Value ?? "", outcome.ResponseXml));
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            consumer.Close();
            producer.Flush(TimeSpan.FromSeconds(5));
        }
    }
}
