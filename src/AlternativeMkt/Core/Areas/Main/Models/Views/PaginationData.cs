namespace AlternativeMkt.Main.Models;

public class PaginationData
{
    public Dictionary<string, string> RouteParams {get; set;} = null!;
    public int CurrentPage { get; set; }
    public int PreviousIndex { get; set; }
    public int CurrentIndex { get; set; }
    public int NextIndex { get; set; }
    public int Count { get; set; }
    public int Total { get; set; }
    public int LastValidIndex { get; set; }
}