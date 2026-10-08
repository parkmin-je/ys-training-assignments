using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbMsgLog
{
    public long LogSeq { get; set; }

    public string Direction { get; set; } = null!;

    public string Topic { get; set; } = null!;

    public int? PartitionNo { get; set; }

    public long? OffsetNo { get; set; }

    public string? MessageId { get; set; }

    public string? EventType { get; set; }

    public string? EqpId { get; set; }

    public string? LotId { get; set; }

    public string RawXml { get; set; } = null!;

    public string? Result { get; set; }

    public string? Reason { get; set; }

    public DateTime LoggedAt { get; set; }
}
