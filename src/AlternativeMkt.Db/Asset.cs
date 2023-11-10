using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlternativeMkt.Db;

public partial class Asset
{
    public int Id { get; set; }

    public string Endpoint { get; set; } = null!;
    public ICollection<CraftItem> CraftItems { get; set; } = new List<CraftItem>();
}
