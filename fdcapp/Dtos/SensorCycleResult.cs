namespace fdcapp.Dtos
{
    /// 시뮬레이터 한 주기의 결과
    public record SensorCycleResult(double TempVal, double PressVal, FdcResult? Result, string? Error);
}