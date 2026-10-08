using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbEqpRun
{
    public long RunId { get; set; }

    public string LotId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string StepId { get; set; } = null!;

    public string EqpId { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public string RunStatus { get; set; } = null!;

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    public string StartMsgId { get; set; } = null!;

    public string? EndMsgId { get; set; }

    public long? CheckId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
