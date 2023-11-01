namespace AlternativeMkt.Api.Models;

public class StandardList<T>
{
    public int Count { get; set; }
    public int Start { get; set; }
    public int Total { get; set; }
    public List<T> Data { get; set; } = new();
}