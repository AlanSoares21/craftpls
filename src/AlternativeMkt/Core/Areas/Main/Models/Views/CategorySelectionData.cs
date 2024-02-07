using AlternativeMkt.Db;

namespace AlternativeMkt.Main.Models;

public class CategorySelectionData
{
    public string Name { get; set; } = "";
    public bool HasOptionAny { get; set; }
    public List<CraftCategory> Categories { get; set; } = new();
    public int CategorySelected { get; set; } = -1;
}