using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbRecipeParam
{
    public string RecipeId { get; set; } = null!;

    public string ParamId { get; set; } = null!;

    public string BaseValue { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual TbRecipe Recipe { get; set; } = null!;
}
