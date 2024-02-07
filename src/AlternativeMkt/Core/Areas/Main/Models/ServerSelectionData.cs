using AlternativeMkt.Db;

namespace AlternativeMkt.Main.Models;

public class ServerSelectionData
{
    public string Name { get; set; } = "";
    public bool HasOptionAny { get; set; }
    public List<Server> Servers { get; set; } = new();
    public int ServerId { get; set; } = -1;
}