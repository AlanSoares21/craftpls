using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class CraftItem
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? CategoryId { get; set; }
    public virtual CraftCategory? Category { get; set; }

    public virtual ICollection<CraftItemsPrice> Prices { get; set; } = new List<CraftItemsPrice>();
    public virtual ICollection<CraftResource> Resources { get; set; } = new List<CraftResource>();
    public virtual ICollection<CraftResource> ResourceFor { get; set; } = new List<CraftResource>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
