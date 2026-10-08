using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbRmsCheck
{
    public long CheckId { get; set; }

    public string MessageId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string LotId { get; set; } = null!;

    public string StepId { get; set; } = null!;

    public string EqpId { get; set; } = null!;

    public string RecvRecipeId { get; set; } = null!;

    public string BaseRecipeId { get; set; } = null!;

    public string Result { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTimeOffset EventTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TbRmsCheckDtl> TbRmsCheckDtls { get; set; } = new List<TbRmsCheckDtl>();
}
