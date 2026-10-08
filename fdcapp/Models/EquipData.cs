using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fdcapp.Models
{
    /// 설비 센서 수집 데이터 (TB_EQUIP_DATA)
    [Table("TB_EQUIP_DATA")]
    public class EquipData
    {
        /// 수집 데이터 일련번호 (자동 증가)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("DATA_ID")]
        public long DataId { get; set; }

        /// 설비 고유 코드
        [Required]
        [Column("EQUIP_ID")]
        [StringLength(200)]
        public string EquipId { get; set; } = string.Empty;

        /// 온도 측정 센서값 (℃)
        [Required]
        [Column("TEMP_VAL")]
        public double TempVal { get; set; }

        /// 압력 측정 센서값 (bar)
        [Required]
        [Column("PRESS_VAL")]
        public double PressVal { get; set; }

        /// 판정 결과 (NORMAL / ALARM)
        [Column("IS_FAULT")]
        [StringLength(200)]
        public string? IsFault { get; set; }

        /// 데이터 수집 일시
        [Column("COLLECT_DT")]
        public DateTime? CollectDt { get; set; }
    }
}