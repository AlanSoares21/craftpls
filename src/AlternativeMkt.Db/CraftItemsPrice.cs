using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class CraftItemsPrice
{
    public Guid Id { get; set; }

    public int Price { get; set; }
    public int TotalPrice { get; set; }

    public int ItemId { get; set; }

    public Guid ManufacturerId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual CraftItem Item { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;
}
