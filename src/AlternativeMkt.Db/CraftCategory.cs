using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class CraftCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<CraftItem> CraftItems { get; set; } = new List<CraftItem>();
}
