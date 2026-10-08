using System;
using System.Collections.Generic;

namespace RmsMasterData.Data.Entities;

public partial class TbEquipment
{
    public string EquipId { get; set; } = null!;

    public string EquipName { get; set; } = null!;

    public string UseYn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TbRmsRecipe> TbRmsRecipes { get; set; } = new List<TbRmsRecipe>();
}
