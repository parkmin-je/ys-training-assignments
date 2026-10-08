using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fdcapp.Models
{
    /// 설비 마스터 (TB_EQUIPMENT)
    [Table("TB_EQUIPMENT")]
    public class Equipment
    {
        /// 설비 고유 코드
        [Key]
        [Column("EQUIP_ID")]
        [StringLength(200)]
        public string EquipId { get; set; } = string.Empty;

        /// 설비명
        [Required]
        [Column("EQUIP_NAME")]
        [StringLength(200)]
        public string EquipName { get; set; } = string.Empty;

        /// 배치 라인명
        [Required]
        [Column("LINE_NAME")]
        [StringLength(200)]
        public string LineName { get; set; } = string.Empty;

        /// 설비 상태 (IDLE, RUN 등)
        [Column("STATUS")]
        [StringLength(200)]
        public string? Status { get; set; }

        /// 생성 일시
        [Column("CREATE_DT")]
        public DateTime? CreateDt { get; set; }
    }
}