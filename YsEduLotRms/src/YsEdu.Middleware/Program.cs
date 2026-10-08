namespace YsEdu.Middleware;

static class Program
{
    /// <summary>
    /// Kafka 연계 미들웨어 — YSEDU.LOT.REQUEST 수신 → 업무 처리(DB) → YSEDU.LOT.RESPONSE 응답
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
