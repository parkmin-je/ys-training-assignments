using System;
using System.Collections.Generic;

namespace RmsKafkaMiddleware.Data.Entities;

public partial class TbRmsRecipe
{
    public string RecipeId { get; set; } = null!;

    public string EquipId { get; set; } = null!;

    public string FactorId { get; set; } = null!;

    public string FactorValue { get; set; } = null!;

    public string UseYn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual TbEquipment Equip { get; set; } = null!;

    public virtual ICollection<TbRmsRecipeParameter> TbRmsRecipeParameters { get; set; } = new List<TbRmsRecipeParameter>();

    public virtual ICollection<TbRmsResult> TbRmsResults { get; set; } = new List<TbRmsResult>();
}
