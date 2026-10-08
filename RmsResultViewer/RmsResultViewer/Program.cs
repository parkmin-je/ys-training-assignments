namespace RmsResultViewer;

static class Program
{
    /// RMS 결과 조회 — Middleware가 저장한 TB_RMS_RESULT(Master)와 TB_RMS_RESULT_PARAMETER(Detail) 조회
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
