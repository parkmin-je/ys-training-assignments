namespace RmsKafkaMiddleware;

static class Program
{
    /// <summary>
    /// Kafka 기반 RMS Middleware — RMS.REQUEST(RMS_TEST XML) 수신 → 레시피·파라미터 조회 → RMS.RESPONSE 응답 → 결과 저장
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
