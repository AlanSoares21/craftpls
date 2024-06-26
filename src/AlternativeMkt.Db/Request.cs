﻿using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class Request
{
    public Guid Id { get; set; }

    public int Price { get; set; }

    public int ItemId { get; set; }

    public Guid RequesterId { get; set; }

    public Guid ManufacturerId { get; set; }

    public DateTime? Cancelled { get; set; }

    public DateTime? Refused { get; set; }

    public DateTime? Accepted { get; set; }

    public DateTime? FinishedByManufacturer { get; set; }

    public DateTime? FinishedByRequester { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual CraftItem Item { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual User Requester { get; set; } = null!;
    public List<ResourceProvided> ResourcesProvided { get; set; } = null!;
}
