namespace AlternativeMkt.Db;

public class ResourceProvided
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public int ResourceId { get; set; }
    public Request Request { get; set; } = new();
    public CraftResource Resource { get; set; } = new();
}