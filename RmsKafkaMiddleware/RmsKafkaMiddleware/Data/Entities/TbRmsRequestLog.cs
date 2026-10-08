using System;
using System.Collections.Generic;

namespace RmsKafkaMiddleware.Data.Entities;

public partial class TbRmsRequestLog
{
    public long LogId { get; set; }

    public string? MessageKey { get; set; }

    public string KafkaTopic { get; set; } = null!;

    public int KafkaPartition { get; set; }

    public long KafkaOffset { get; set; }

    public string? EquipId { get; set; }

    public string ResultCode { get; set; } = null!;

    public string? ResultMessage { get; set; }

    public long? ResultId { get; set; }

    public string RequestXml { get; set; } = null!;

    public string? ResponseXml { get; set; }

    public DateTime ReceivedAt { get; set; }
}
