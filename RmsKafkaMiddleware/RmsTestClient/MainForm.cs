using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace RmsTestClient;

/// 수신한 응답 1건
public sealed record ResponseRow(DateTime Time, string ResultCode, string? EqpId, string? RecipeId, int ParameterCount, string ResultMessage, string Xml);

/// <summary>
/// RMS_TEST 요청 송신 도구
///   · Samples 폴더의 요청 예제(정상 1 + 오류 5)를 골라 RMS.REQUEST로 보낸다
///   · RMS.RESPONSE를 구독해 응답 XML을 표시한다
///   · 같은 XML을 두 번 보내면 미들웨어가 DUPLICATE로 응답하는지 확인할 수 있다
/// </summary>
public partial class MainForm : Form
{
    private readonly BindingList<ResponseRow> _responses = new();
    private readonly CancellationTokenSource _cts = new();
    private IProducer<string?, string>? _producer;
    private string _requestTopic = "", _responseTopic = "";
    private Task? _responseLoop;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        dgvResponses.DataSource = _responses;
        foreach (var file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Samples"), "*.xml").Order())
            lstSamples.Items.Add(Path.GetFileName(file));

        try
        {
            var c = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
            string servers = c["Kafka:BootstrapServers"]!;
            _requestTopic = c["Kafka:RequestTopic"]!;
            _responseTopic = c["Kafka:ResponseTopic"]!;

            _producer = new ProducerBuilder<string?, string>(new ProducerConfig { BootstrapServers = servers, Acks = Acks.All }).Build();
            await StartResponseConsumerAsync(servers);
            lblConnection.Text = $"● {servers}\n송신 {_requestTopic} / 수신 {_responseTopic}";
            lblConnection.ForeColor = Color.ForestGreen;
        }
        catch (Exception ex)
        {
            lblConnection.Text = "× Kafka 연결 실패";
            lblConnection.ForeColor = Color.Firebrick;
            btnSend.Enabled = false;
            MessageBox.Show(ex.Message, "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        if (lstSamples.Items.Count > 0) lstSamples.SelectedIndex = 0;
    }

    /// 응답 Consumer: 이 프로그램 전용 그룹으로 최신 응답부터 받는다. Partition 할당까지 기다린다
    private async Task StartResponseConsumerAsync(string servers)
    {
        var assigned = new TaskCompletionSource();
        var consumer = new ConsumerBuilder<string?, string>(new ConsumerConfig
        {
            BootstrapServers = servers,
            GroupId = $"rms-test-client-{Guid.NewGuid():N}",
            AutoOffsetReset = AutoOffsetReset.Latest
        })
        .SetPartitionsAssignedHandler((_, _) => assigned.TrySetResult())
        .Build();
        consumer.Subscribe(_responseTopic);

        var token = _cts.Token;
        _responseLoop = Task.Run(() =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var cr = consumer.Consume(TimeSpan.FromMilliseconds(300));
                    if (cr != null) OnResponse(cr.Message.Value);
                }
            }
            finally
            {
                consumer.Close();
                consumer.Dispose();
            }
        }, token);

        await Task.WhenAny(assigned.Task, Task.Delay(TimeSpan.FromSeconds(15)));
    }

    private void lstSamples_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lstSamples.SelectedItem is not string name) return;
        string xml = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Samples", name));
        if (chkNowTime.Checked)
            xml = Regex.Replace(xml, "<EVENT_TIME>.*?</EVENT_TIME>", $"<EVENT_TIME>{DateTime.Now:yyyy-MM-ddTHH:mm:ss.fff}</EVENT_TIME>");
        txtRequest.Text = xml.Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    private async void btnSend_Click(object sender, EventArgs e)
    {
        if (_producer == null || string.IsNullOrWhiteSpace(txtRequest.Text)) return;
        btnSend.Enabled = false;
        try
        {
            string? key = Regex.Match(txtRequest.Text, "<EQP_ID>(.*?)</EQP_ID>").Groups[1].Value is { Length: > 0 } k ? k : null;
            await _producer.ProduceAsync(_requestTopic, new Message<string?, string> { Key = key, Value = txtRequest.Text });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "송신 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSend.Enabled = true;
        }
    }

    /// 응답 Consumer 스레드 → UI 스레드
    private void OnResponse(string xml)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<string>(OnResponse), xml);
            return;
        }

        string code = "?", message = "", eqp = "", recipe = "";
        int count = 0;
        try
        {
            var root = XElement.Parse(xml);
            code = root.Element("HEADER")?.Element("RESULT_CODE")?.Value ?? "?";
            message = root.Element("HEADER")?.Element("RESULT_MESSAGE")?.Value ?? "";
            eqp = root.Element("BODY")?.Element("EQP_ID")?.Value ?? "";
            recipe = root.Element("BODY")?.Element("RECIPE_ID")?.Value ?? "";
            count = root.Element("BODY")?.Element("PARAMETERS")?.Elements("PARAMETER").Count() ?? 0;
        }
        catch (System.Xml.XmlException)
        {
            message = "응답 XML을 읽지 못함";
        }

        _responses.Insert(0, new ResponseRow(DateTime.Now, code, eqp, recipe, count, message, xml));
        dgvResponses.ClearSelection();
        dgvResponses.Rows[0].Selected = true;
        txtResponse.Text = xml.Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    private void dgvResponses_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvResponses.CurrentRow?.DataBoundItem is ResponseRow row)
            txtResponse.Text = row.Xml.Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        _cts.Cancel();
        try { _responseLoop?.Wait(2000); } catch (AggregateException) { }
        _producer?.Flush(TimeSpan.FromSeconds(2));
        _producer?.Dispose();
    }
}
