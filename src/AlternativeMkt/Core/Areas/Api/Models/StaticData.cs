
using AlternativeMkt.Db;

namespace AlternativeMkt.Api.Models;

public class StaticData
{
    public List<CraftCategory> Categories { get; set; } = new();
    public List<Db.Attribute> Attributes { get; set; } = new();
}