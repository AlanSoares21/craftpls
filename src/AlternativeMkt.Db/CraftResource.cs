using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class CraftResource
{
    public int Id { get; set; }

    public short? Amount { get; set; }
    public int ResourceId { get; set; }

    public virtual CraftItem Resource { get; set; } = null!;

    public int ItemId { get; set; }

    public virtual CraftItem Item { get; set; } = null!;
}
