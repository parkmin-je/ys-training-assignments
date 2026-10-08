using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbStep
{
    public string StepId { get; set; } = null!;

    public string StepName { get; set; } = null!;

    public string UseYn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TbRecipe> TbRecipes { get; set; } = new List<TbRecipe>();
}
