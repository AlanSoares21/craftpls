namespace AlternativeMkt.Db;

public partial class Server
{
    public byte Id { get; set; }
    public string Name { get; set; } = "";
    public virtual List<GameAccount>? GameAccounts { get; set; }

    public virtual List<Manufacturer>? Manufacturers { get; set; }
}