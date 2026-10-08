namespace YsEdu.Viewer;

static class Program
{
    /// <summary>
    /// WinForm 조회 화면 — LOT 작업·설비 구동 이력과 RMS 검증 상세를 DB에서 조회한다
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
