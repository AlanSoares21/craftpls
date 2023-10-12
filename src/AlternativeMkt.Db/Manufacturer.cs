using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class Manufacturer
{
    public Guid Id { get; set; }

    public short MaxRequestsOpen { get; set; }

    public short MaxRequestsAccepted { get; set; }

    public bool Hide { get; set; }

    public Guid Userid { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<CraftItemsPrice> CraftItemsPrices { get; set; } = new List<CraftItemsPrice>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual User User { get; set; } = null!;
}
