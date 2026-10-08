using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbMsgProcessed
{
    public string MessageId { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string EqpId { get; set; } = null!;

    public string Result { get; set; } = null!;

    public string? Reason { get; set; }

    public DateTime ProcessedAt { get; set; }
}
