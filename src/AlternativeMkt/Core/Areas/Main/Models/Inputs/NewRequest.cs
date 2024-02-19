namespace AlternativeMkt.Main.Models;

public class NewRequest
{
    public Guid PriceId { get; set; }
    public List<int> ProvidedResources { get; set; } = new();
}