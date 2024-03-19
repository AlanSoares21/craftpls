using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AlternativeMkt.Db;

public partial class CraftItemDataByCulture
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Culture { get; set; } = null!;
    public int ItemId { get; set; }
    public CraftItem Item { get; set; } = new();
}
