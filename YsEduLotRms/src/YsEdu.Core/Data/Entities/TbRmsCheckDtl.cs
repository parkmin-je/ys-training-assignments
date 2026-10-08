using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbRmsCheckDtl
{
    public long DtlSeq { get; set; }

    public long CheckId { get; set; }

    public string ItemType { get; set; } = null!;

    public string ItemId { get; set; } = null!;

    public string? BaseValue { get; set; }

    public string? RecvValue { get; set; }

    public string Judge { get; set; } = null!;

    public string? Reason { get; set; }

    public virtual TbRmsCheck Check { get; set; } = null!;
}
