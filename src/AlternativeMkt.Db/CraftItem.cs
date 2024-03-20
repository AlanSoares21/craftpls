using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class CraftItem
{
    public int Id { get; set; }

    public string? Name { get; set; }
    public int? Level { get; set; }

    public int? CategoryId { get; set; }
    public CraftCategory? Category { get; set; }
    public int? AssetId { get; set; }
    public List<CraftItemDataByCulture> DataByCulture { get; set; } = new();

    public ICollection<CraftItemsPrice> Prices { get; set; } = new List<CraftItemsPrice>();
    public ICollection<CraftResource> Resources { get; set; } = new List<CraftResource>();
    public ICollection<CraftResource> ResourceFor { get; set; } = new List<CraftResource>();

    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public Asset? Asset { get; set; }
    public List<CraftItemAttribute> Attributes { get; set; } = new();
}
