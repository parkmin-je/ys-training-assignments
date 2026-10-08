using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbLot
{
    public string LotId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string StepId { get; set; } = null!;

    public string LotStatus { get; set; } = null!;

    public string? CurEqpId { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public string? StartMsgId { get; set; }

    public string? EndMsgId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
