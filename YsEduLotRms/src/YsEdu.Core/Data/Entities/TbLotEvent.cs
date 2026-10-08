using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbLotEvent
{
    public long EventSeq { get; set; }

    public string MessageId { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string LotId { get; set; } = null!;

    public string StepId { get; set; } = null!;

    public string EqpId { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public DateTimeOffset EventTime { get; set; }

    public string Result { get; set; } = null!;

    public string? Reason { get; set; }

    public long? RunId { get; set; }

    public long? CheckId { get; set; }

    public DateTime CreatedAt { get; set; }
}
