namespace YsEdu.Simulator;

static class Program
{
    /// <summary>
    /// 설비 / CIM 시뮬레이터 — A · B · C 인라인 설비가 YSEDU.LOT.REQUEST로 이벤트를 보내고 응답을 받는다
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
