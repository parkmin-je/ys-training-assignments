using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using RmsKafkaMiddleware.Common;
using RmsKafkaMiddleware.Messaging;
using RmsKafkaMiddleware.Services;

// ============================================================================
// Kafka RMS Middleware 검증 — 실제 Kafka(RMS.REQUEST/RESPONSE)와 DB(YsRmsDB)
//   정상 1건 · 필수 오류 5종 · 동일 메시지 중복 → 응답 RESULT_CODE와 저장 결과 확인
// ============================================================================
Console.OutputEncoding = System.Text.Encoding.UTF8;
var settings = AppSettings.Load();
var log = new FileLog(settings.LogDirectory);
int pass = 0, fail = 0;
void Check(bool ok, string text) { if (ok) pass++; else fail++; Console.WriteLine($"{(ok ? "  ✔" : "  ✘")} {text}"); }

// 미들웨어를 이 프로그램 안에서 실행
await using var worker = new KafkaWorker(settings, log);
worker.Start();

// 응답 수신 (RESULT_CODE 순서대로 큐에 쌓음)
var responses = new BlockingCollection<string>();
var assigned = new TaskCompletionSource();
using var consumer = new ConsumerBuilder<string?, string>(new ConsumerConfig
{
    BootstrapServers = settings.BootstrapServers, GroupId = $"rms-test-{Guid.NewGuid():N}", AutoOffsetReset = AutoOffsetReset.Latest
}).SetPartitionsAssignedHandler((_, _) => assigned.TrySetResult()).Build();
consumer.Subscribe(settings.ResponseTopic);
var cts = new CancellationTokenSource();
var loop = Task.Run(() => { while (!cts.IsCancellationRequested) { var cr = consumer.Consume(300); if (cr != null) responses.Add(cr.Message.Value); } });
await assigned.Task.WaitAsync(TimeSpan.FromSeconds(20));
await Task.Delay(1500);

using var producer = new ProducerBuilder<string?, string>(new ProducerConfig { BootstrapServers = settings.BootstrapServers }).Build();
string now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
string Sample(string file) => Regex.Replace(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Samples", file)),
                                            "<EVENT_TIME>.*?</EVENT_TIME>", $"<EVENT_TIME>{now}</EVENT_TIME>");

async Task<XElement?> Send(string xml)
{
    await producer.ProduceAsync(settings.RequestTopic, new Message<string?, string> { Key = "TEST", Value = xml });
    return responses.TryTake(out var r, TimeSpan.FromSeconds(15)) ? XElement.Parse(r) : null;
}
string Code(XElement? r) => r?.Element("HEADER")?.Element("RESULT_CODE")?.Value ?? "응답 없음";

Console.WriteLine("■ 정상 요청");
var ok = await Send(Sample("01_success.xml"));
Check(Code(ok) == "SUCCESS" && ok!.Element("BODY")!.Element("RECIPE_ID")!.Value == "RECIPE_001"
      && ok.Element("BODY")!.Element("PARAMETERS")!.Elements("PARAMETER").Count() == 2,
      $"EQP_001 / MODEL / A100 → SUCCESS, RECIPE_001, 파라미터 2건 ({Code(ok)})");

Console.WriteLine("\n■ 필수 오류 처리 (Kafka p.11)");
foreach (var (file, code) in new[] {
    ("02_xml_format_error.xml", "XML_FORMAT_ERROR"), ("03_missing_node.xml", "MISSING_NODE"),
    ("04_rule_mismatch.xml", "RULE_NOT_SUPPORTED"), ("05_recipe_not_found.xml", "RECIPE_NOT_FOUND"),
    ("06_parameter_not_found.xml", "PARAMETER_NOT_FOUND") })
{
    var r = await Send(Sample(file));
    Check(Code(r) == code, $"{file,-28} → {Code(r)}  {r?.Element("HEADER")?.Element("RESULT_MESSAGE")?.Value}");
}

Console.WriteLine("\n■ 동일 메시지 중복");
var dup = await Send(Sample("01_success.xml"));
Check(Code(dup) == "DUPLICATE", $"같은 요청 재송신 → {Code(dup)}");

Console.WriteLine("\n■ DB 저장 결과");
await using (var db = settings.CreateDbContext())
{
    var results = await db.TbRmsResults.Include(r => r.TbRmsResultParameters)
        .Where(r => r.EventTime == DateTime.Parse(now)).ToListAsync();
    Check(results.Count == 1, $"요청 1건당 Master 1건 (중복 재송신 포함 {results.Count}건)");
    var m = results.FirstOrDefault();
    Check(m is { EquipId: "EQP_001", RecipeId: "RECIPE_001" } && m.TbRmsResultParameters.Count == 2,
          $"Master EVENT_TIME·EQUIP_ID·RECIPE_ID 저장, Detail {m?.TbRmsResultParameters.Count}건 RESULT_ID로 연결");
    var logs = await db.TbRmsRequestLogs.Where(l => l.ReceivedAt >= DateTime.Now.AddMinutes(-2)).Select(l => l.ResultCode).ToListAsync();
    Check(logs.Count(c => c != "SUCCESS" && c != "DUPLICATE") >= 5, $"오류 결과 기록 (TB_RMS_REQUEST_LOG 오류 {logs.Count(c => c != "SUCCESS" && c != "DUPLICATE")}건)");
}

cts.Cancel(); await loop;
await worker.StopAsync();
Check(!worker.IsRunning, "중지 시 수신 작업 종료");
Console.WriteLine($"\nRESULT: {(fail == 0 ? "PASS" : "FAIL")}  (통과 {pass} / 실패 {fail})");
return fail == 0 ? 0 : 1;
