using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class GameAccount
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public Guid UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
