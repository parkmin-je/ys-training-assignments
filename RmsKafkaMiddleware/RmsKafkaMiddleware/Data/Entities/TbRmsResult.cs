using System;
using System.Collections.Generic;

namespace RmsKafkaMiddleware.Data.Entities;

public partial class TbRmsResult
{
    public long ResultId { get; set; }

    public DateTime EventTime { get; set; }

    public string EquipId { get; set; } = null!;

    public string RecipeId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual TbEquipment Equip { get; set; } = null!;

    public virtual TbRmsRecipe Recipe { get; set; } = null!;

    public virtual ICollection<TbRmsResultParameter> TbRmsResultParameters { get; set; } = new List<TbRmsResultParameter>();
}
