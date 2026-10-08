using fdcapp.Data;
using fdcapp.Dtos;
using fdcapp.Models;

namespace fdcapp.Services
{
    public class EquipmentService : IEquipmentService
    {

        private readonly AppDbContext _dbContext;

        public EquipmentService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // DB에서 전체 설비 목록을 조회함
        public List<Equipment> GetEquipmentList()
        {

            List<Equipment> equipmentList = _dbContext.Equipments
                .OrderBy(equipment => equipment.EquipId)
                .ToList();
            
            return equipmentList;

        }

        // 중복 확인 후 새 설비를 DB에 추가함
        public void AddEquipment(Equipment item)
        {

            if (_dbContext.Equipments.Find(item.EquipId) != null)
            {
                throw new InvalidOperationException($"이미 존재하는 설비입니다: {item.EquipId}");
            }

            if (item.CreateDt == null)
            {
                item.CreateDt = DateTime.Now;
            }

            _dbContext.Equipments.Add(item);
            _dbContext.SaveChanges();

        }

        // 특정 설비의 로그를 발생일시 최신순으로 조회함
        public List<EquipLog> GetLogsByEquipId(string equipId)
        {

            List<EquipLog> logList = _dbContext.EquipLogs
                .Where(log => log.EquipId == equipId)
                .OrderByDescending(log => log.OccurDt)
                .ToList();
                
            return logList;

        }

        // 설비 상태를 새 값으로 변경하고 변경 이력을 로그에 남김
        public void UpdateEquipmentStatus(string id, string newStatus)
        {

            Equipment? equipment = _dbContext.Equipments.Find(id) ?? throw new InvalidOperationException($"설비를 찾을 수 없습니다: {id}");

            string oldStatus = equipment.Status ?? "UNKNOWN";

            equipment.Status = newStatus;

            EquipLog log = new EquipLog
            {
                EquipId = id,
                LogType = "STATUS_CHG",
                LogMsg = $"상태 변경 [{oldStatus} ➔ {newStatus}]",
                OccurDt = DateTime.Now
            };
            _dbContext.EquipLogs.Add(log);

            _dbContext.SaveChanges();

        }

        // 측정값을 임계치 규칙과 비교해 알람 여부를 판정함
        public FdcResult CheckParameterAlarm(string equipId, string paramName, double val)
        {
            // 판정 로직 수행 (변경 사항은 추적기에만 반영)
            FdcResult result = EvaluateParameterAlarm(equipId, paramName, val);

            // 상태 변경 + 로그 추가를 저장
            _dbContext.SaveChanges();

            return result;
        }

        // 임계치 규칙과 비교해 판정 결과와 상태/로그 변경을 추적기에 반영함 (저장은 호출부에서)
        private FdcResult EvaluateParameterAlarm(string equipId, string paramName, double val)
        {

            // 해당 설비+파라미터의 임계치 규칙 조회
            ParamLimit? rule = _dbContext.ParamLimits
                .FirstOrDefault(r => r.EquipId == equipId && r.ParamName == paramName);

            // 규칙이 없으면 감사 로그를 남기고 예외 발생 (미감시 상태를 정상으로 오판하지 않음)
            if (rule == null)
            {
                EquipLog noRuleLog = new EquipLog
                {
                    EquipId = equipId,
                    LogType = "NO_RULE",
                    LogMsg = $"임계치 규칙 미설정: {equipId}/{paramName}",
                    OccurDt = DateTime.Now
                };
                _dbContext.EquipLogs.Add(noRuleLog);
                _dbContext.SaveChanges();

                throw new InvalidOperationException($"임계치 규칙이 없습니다: {equipId}/{paramName}");
            }

            // 정상 범위 판정
            if (val >= rule.LowerLimit && val <= rule.UpperLimit)
            {
                return new FdcResult(false, $"정상 범위 ({rule.LowerLimit} ~ {rule.UpperLimit})");
            }

            // 상한/하한 구분 및 메시지 생성
            bool isUpper = val > rule.UpperLimit;
            string paramLabel = paramName switch
            {
                "TEMP" => "온도",
                "PRESSURE" => "압력",
                _ => paramName
            };

            string unit = paramName switch
            {
                "TEMP" => "℃",
                "PRESSURE" => "bar",
                _ => ""
            };

            string limitLabel = isUpper ? "상한" : "하한";
            string direction = isUpper ? "초과" : "미달";
            double limitVal = isUpper ? rule.UpperLimit : rule.LowerLimit;
            string message = $"{paramLabel} {limitLabel}({limitVal}{unit}) {direction} 발생: {val}{unit}";

            // 설비 상태 DOWN 변경
            Equipment? equipment = _dbContext.Equipments.Find(equipId) ?? throw new InvalidOperationException($"설비를 찾을 수 없습니다: {equipId}");
            equipment.Status = "DOWN";

            // ALARM 로그 추가
            EquipLog log = new EquipLog
            {
                EquipId = equipId,
                LogType = "ALARM",
                LogMsg = message,
                OccurDt = DateTime.Now
            };
            
            _dbContext.EquipLogs.Add(log);

            return new FdcResult(true, message);

        }

        // 센서 수집값을 TB_EQUIP_DATA에 기록하고 FDC 판정을 수행함
        public FdcResult CollectSensorData(string equipId, double tempVal, double pressVal)
        {

            // 온도 기준 FDC 판정 (변경 사항은 추적기에만 반영, 아직 저장 안 함)
            FdcResult result = EvaluateParameterAlarm(equipId, "TEMP", tempVal);

            // 수집 데이터 INSERT (IS_FAULT에 판정 결과 반영)
            EquipData data = new EquipData
            {
                EquipId = equipId,
                TempVal = tempVal,
                PressVal = pressVal,
                IsFault = result.IsAlarm ? "ALARM" : "NORMAL",
                CollectDt = DateTime.Now
            };

            _dbContext.EquipDatas.Add(data);

            _dbContext.SaveChanges();

            return result;

        }
    }
}