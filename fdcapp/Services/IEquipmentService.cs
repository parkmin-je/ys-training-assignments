using fdcapp.Dtos;
using fdcapp.Models;

namespace fdcapp.Services
{
    public interface IEquipmentService
    {
        List<Equipment> GetEquipmentList();
        void AddEquipment(Equipment item);
        List<EquipLog> GetLogsByEquipId(string equipId);
        void UpdateEquipmentStatus(string id, string newStatus);
        FdcResult CheckParameterAlarm(string equipId, string paramName, double val);
        FdcResult CollectSensorData(string equipId, double tempVal, double pressVal);
    }
}