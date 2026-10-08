using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbRecipe
{
    public string RecipeId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public string StepId { get; set; } = null!;

    public string EqpId { get; set; } = null!;

    public string UseYn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TbEquipment Eqp { get; set; } = null!;

    public virtual TbProduct Product { get; set; } = null!;

    public virtual TbStep Step { get; set; } = null!;

    public virtual ICollection<TbRecipeParam> TbRecipeParams { get; set; } = new List<TbRecipeParam>();
}
