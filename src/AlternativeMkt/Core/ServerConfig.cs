using System.Security.Claims;
using System.Text;

namespace AlternativeMkt;

public class ServerConfig
{
    private IConfiguration _configuration;
    public ServerConfig(IConfiguration configuration) {
        _configuration = configuration;
    }
    private int _SecondsAuthTokenExpireDefault = 60;
    private bool _SecondsAuthTokenExpireIsEmpty() => 
        string.IsNullOrEmpty(_configuration["Jwt:SecondsAuthTokenExpire"]);
    public string Issuer => "" + _configuration["Jwt:Issuer"];

    public byte[] SecretKey => 
        Encoding.UTF8.GetBytes("" + _configuration["Jwt:Secret"]);

    public string Audience => "" + _configuration["Jwt:Audience"];

    public int SecondsAuthTokenExpire => 
        _SecondsAuthTokenExpireIsEmpty() ?
        _SecondsAuthTokenExpireDefault :
        int.Parse("" + _configuration["Jwt:SecondsAuthTokenExpire"]);
    
    public int SecondsRefreshTokenExpire => 1800;

    public string AllowedOrigin => "" + _configuration["Jwt:AllowedOrigin"];
    public int AdminRoleId => int.Parse("" + _configuration["Roles:Admin"]);
    public int DevRoleId => int.Parse("" + _configuration["Roles:Dev"]);
    public string? PriceDashboardJs => _configuration["PriceDashboardJs"];
    public string? AdminItemsPageJs => _configuration["AdminItemsPageJs"];
    public string? AssetsUrl => _configuration["AssetsUrl"];
}