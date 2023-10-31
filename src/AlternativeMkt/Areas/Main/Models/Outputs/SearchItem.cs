namespace AlternativeMkt.Main.Models;

public class SearchItem
{
    public string name { get; set; } = null!;
    public List<string> manufacturerAccounts { get; set; } = new();
    public int price { get; set; } = 0;
}