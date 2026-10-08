using System;
using System.Collections.Generic;

namespace YsEdu.Core.Data.Entities;

public partial class TbProduct
{
    public string ProductId { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string UseYn { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TbRecipe> TbRecipes { get; set; } = new List<TbRecipe>();
}
