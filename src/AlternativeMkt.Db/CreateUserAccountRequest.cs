namespace AlternativeMkt.Db;

public class CreateUserAccountRequest
{
    public string Email { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
}