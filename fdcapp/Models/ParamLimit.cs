using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fdcapp.Models
{
    /// 파라미터 임계치 규칙 (TB_PARAM_LIMIT)
    [Table("TB_PARAM_LIMIT")]
    public class ParamLimit
    {
        /// 임계치 규칙 번호 (자동 증가)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("LIMIT_ID")]
        public long LimitId { get; set; }

        /// 설비 고유 코드
        [Required]
        [Column("EQUIP_ID")]
        [StringLength(200)]
        public string EquipId { get; set; } = string.Empty;

        /// 측정 파라미터 (TEMP, PRESSURE)
        [Required]
        [Column("PARAM_NAME")]
        [StringLength(200)]
        public string ParamName { get; set; } = string.Empty;

        /// 정상 하한값 (예: 20.0)
        [Required]
        [Column("LOWER_LIMIT")]
        public double LowerLimit { get; set; }

        /// 정상 상한값 (예: 80.0)
        [Required]
        [Column("UPPER_LIMIT")]
        public double UpperLimit { get; set; }

        /// 룰 등록 일시
        [Column("CREATE_DT")]
        public DateTime? CreateDt { get; set; }
    }
}