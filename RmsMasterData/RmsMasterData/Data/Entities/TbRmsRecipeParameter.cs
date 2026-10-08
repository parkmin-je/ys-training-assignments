using System;
using System.Collections.Generic;

namespace RmsMasterData.Data.Entities;

public partial class TbRmsRecipeParameter
{
    public string RecipeId { get; set; } = null!;

    public string ParameterId { get; set; } = null!;

    public string ParameterValue { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TbRmsRecipe Recipe { get; set; } = null!;
}
