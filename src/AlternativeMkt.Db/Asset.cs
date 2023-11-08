using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class Asset
{
    public int Id { get; set; }

    public string Endpoint { get; set; } = null!;

    public virtual List<CraftItem> CraftItems { get; set; } = new();
}
