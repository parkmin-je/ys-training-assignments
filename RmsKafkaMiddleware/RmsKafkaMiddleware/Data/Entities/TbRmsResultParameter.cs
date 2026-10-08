using System;
using System.Collections.Generic;

namespace RmsKafkaMiddleware.Data.Entities;

public partial class TbRmsResultParameter
{
    public long ResultId { get; set; }

    public string ParameterId { get; set; } = null!;

    public string ParameterValue { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual TbRmsResult Result { get; set; } = null!;
}
