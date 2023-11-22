using System;
using System.Collections.Generic;

namespace AlternativeMkt.Db;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<UserRole> Users { get; set; } = new List<UserRole>();
}
