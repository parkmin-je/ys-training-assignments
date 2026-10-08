namespace RmsTestClient;

static class Program
{
    /// <summary>
    /// RMS_TEST 요청 송신 도구 — 요청을 보내는 쪽(설비/상위 시스템)을 대신한다.
    /// 미들웨어 프로젝트를 참조하지 않고 Kafka와 XML만으로 통신한다.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
