namespace AlternativeMkt.Auth;

public class UserAuthData
{
    public Guid Id { get; set; } = Guid.Empty;
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

}