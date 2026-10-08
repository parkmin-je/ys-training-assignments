using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fdcapp.Models
{
    /// 설비 로그 (TB_EQUIP_LOG)
    [Table("TB_EQUIP_LOG")]
    public class EquipLog
    {
        /// 로그 ID (자동 증가)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("LOG_ID")]
        public long LogId { get; set; }

        /// 설비 고유 코드
        [Column("EQUIP_ID")]
        [StringLength(200)]
        public string? EquipId { get; set; }

        /// 로그 유형
        [Column("LOG_TYPE")]
        [StringLength(200)]
        public string? LogType { get; set; }

        /// 로그 메시지
        [Column("LOG_MSG")]
        [StringLength(200)]
        public string? LogMsg { get; set; }

        /// 발생 일시
        [Column("OCCUR_DT")]
        public DateTime? OccurDt { get; set; }
    }
}