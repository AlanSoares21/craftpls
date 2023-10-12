using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public partial class User
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<GameAccount> GameAccounts { get; set; } = new List<GameAccount>();

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
