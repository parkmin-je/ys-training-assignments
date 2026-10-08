using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbEquipment
{
    public string EqpId { get; set; } = null!;

    public string EqpName { get; set; } = null!;

    public string LineId { get; set; } = null!;

    public string EqpStatus { get; set; } = null!;

    public string? CurLotId { get; set; }

    public string UseYn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<TbRecipe> TbRecipes { get; set; } = new List<TbRecipe>();
}
