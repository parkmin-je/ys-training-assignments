using fdcapp.Dtos;
using fdcapp.Models;

namespace fdcapp.Services
{
    /// <summary>
    /// 가상 센서 시뮬레이터 - 1.5초 주기로 수집/판정을 반복하는 백그라운드 엔진
    /// </summary>
    public class SensorSimulator
    {
        // 수집/판정을 담당하는 서비스
        private readonly IEquipmentService _service;

        // 난수 생성기
        private readonly Random _random = new();

        // 서비스를 주입받아 시뮬레이터 초기화
        public SensorSimulator(IEquipmentService service)
        {
            _service = service;
        }

        // 취소 토큰이 걸릴 때까지 1.5초 주기로 가상 센서 수집을 반복함
        public async Task RunCollector(string equipId, CancellationToken token, Action<SensorCycleResult> onCycle)
        {
            while (!token.IsCancellationRequested)
            {
                // 가상 온도(20~95℃), 압력(1~6bar) 난수 생성
                double tempVal = Math.Round(_random.NextDouble() * 75 + 20, 2);
                double pressVal = Math.Round(_random.NextDouble() * 5 + 1, 2);

                try
                {
                    // 수집 INSERT + FDC 판정
                    FdcResult result = _service.CollectSensorData(equipId, tempVal, pressVal);

                    // UI 갱신 콜백
                    onCycle?.Invoke(new SensorCycleResult(tempVal, pressVal, result, null));
                }
                catch (Exception ex)
                {
                    // 예외 발생 시 콜백으로 알리고 루프 종료 (규칙 미설정 등)
                    onCycle?.Invoke(new SensorCycleResult(tempVal, pressVal, null, ex.Message));
                    break;
                }

                try
                {
                    await Task.Delay(1500, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}