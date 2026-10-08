using System.Collections.Concurrent;
using Confluent.Kafka;
using YsEdu.Core.Config;
using YsEdu.Core.Messaging;

namespace YsEdu.Core.Simulation;

/// 송수신 기록 1건 (화면 표시용)
public sealed record TraceItem(DateTime Time, string Direction, string EqpId, string MessageName, string EventType,
                               string Result, string Reason, string Xml);

/// <summary>
/// 설비 / CIM 시뮬레이터의 통신부.
///   Producer : 요청 XML을 YSEDU.LOT.REQUEST로 송신 (Key = EQPID)
///   Consumer : YSEDU.LOT.RESPONSE를 읽어 MessageName으로 요청과 응답을 연결
/// </summary>
public sealed class EquipmentClient : IAsyncDisposable
{
    private readonly AppSettings _settings;
    private readonly IProducer<string, string> _producer;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<TaskCompletionSource<ProductionEventResponse>>> _waiting = new();
    private readonly CancellationTokenSource _cts = new();
    private Task? _responseLoop;
    private int _seq;

    public EquipmentClient(AppSettings settings)
    {
        _settings = settings;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        }).Build();
    }

    /// 송신·수신할 때마다 발생 (통신 스레드에서 호출됨)
    public event Action<TraceItem>? Traced;

    /// 응답 Consumer를 시작하고, Partition 할당이 끝날 때까지 기다린다 (할당 전 응답을 놓치지 않게)
    public async Task StartAsync()
    {
        var assigned = new TaskCompletionSource();
        var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = $"{_settings.SimulatorGroupId}-{Environment.ProcessId}-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true
        })
        .SetPartitionsAssignedHandler((_, _) => assigned.TrySetResult())
        .Build();
        consumer.Subscribe(_settings.ResponseTopic);

        var token = _cts.Token;
        _responseLoop = Task.Run(() =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var cr = consumer.Consume(TimeSpan.FromMilliseconds(300));
                    if (cr == null) continue;
                    var response = XmlMessage.TryParseResponse(cr.Message.Value);
                    if (response == null) continue;

                    Traced?.Invoke(new TraceItem(DateTime.Now, "↓ 응답", response.EqpId, response.MessageName, response.EventType,
                                                 response.Result, response.Reason, cr.Message.Value));
                    if (_waiting.TryGetValue(response.MessageName, out var queue) && queue.TryDequeue(out var tcs))
                        tcs.TrySetResult(response);
                }
            }
            finally
            {
                consumer.Close();
                consumer.Dispose();
            }
        }, token);

        if (await Task.WhenAny(assigned.Task, Task.Delay(TimeSpan.FromSeconds(20))) != assigned.Task)
            throw new InvalidOperationException("응답 Topic Partition 할당 대기 시간 초과 — Kafka가 실행 중인지 확인하세요.");
        await Task.Delay(300);
    }

    /// 메시지마다 새 MessageName (2차 p.7 "이벤트마다 새 MessageName")
    public string NewMessageName(string eqpId) => $"MSG_{eqpId[^1..]}_{DateTime.Now:HHmmssfff}_{Interlocked.Increment(ref _seq):D4}";

    /// 요청 객체 → XML → 송신 → 응답 대기
    public Task<ProductionEventResponse?> SendAsync(ProductionEvent e)
        => SendRawAsync(e.MessageName, e.EqpId, e.EventType, XmlMessage.Build(e));

    /// 원문 XML 그대로 송신 (잘못된 XML 시험용). waitKey = 응답에서 기대하는 MessageName
    public async Task<ProductionEventResponse?> SendRawAsync(string waitKey, string eqpId, string eventType, string xml)
    {
        var tcs = new TaskCompletionSource<ProductionEventResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        _waiting.GetOrAdd(waitKey, _ => new ConcurrentQueue<TaskCompletionSource<ProductionEventResponse>>()).Enqueue(tcs);

        await _producer.ProduceAsync(_settings.RequestTopic, new Message<string, string> { Key = eqpId, Value = xml });
        Traced?.Invoke(new TraceItem(DateTime.Now, "↑ 요청", eqpId, waitKey, eventType, "", "", xml));

        var done = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(_settings.ResponseTimeoutSeconds)));
        if (done == tcs.Task) return tcs.Task.Result;

        Traced?.Invoke(new TraceItem(DateTime.Now, "× 시간초과", eqpId, waitKey, eventType, "", "응답 없음 — 미들웨어 실행 여부 확인", ""));
        return null;
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        if (_responseLoop != null)
        {
            try { await _responseLoop; } catch (OperationCanceledException) { }
        }
        _producer.Flush(TimeSpan.FromSeconds(3));
        _producer.Dispose();
        _cts.Dispose();
    }
}
