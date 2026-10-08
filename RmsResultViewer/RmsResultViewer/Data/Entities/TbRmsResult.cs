using System;
using System.Collections.Generic;

namespace RmsResultViewer.Data.Entities;

public partial class TbRmsResult
{
    public long ResultId { get; set; }

    public DateTime EventTime { get; set; }

    public string EquipId { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TbRmsResultParameter> TbRmsResultParameters { get; set; } = new List<TbRmsResultParameter>();
}
