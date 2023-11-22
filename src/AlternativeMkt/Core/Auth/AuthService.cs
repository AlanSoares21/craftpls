using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using AlternativeMkt.Db;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace AlternativeMkt.Auth;

public class AuthService: IAuthService
{
    ServerConfig _config;
    IDistributedCache _cache;

    public AuthService(
        IDistributedCache cache,
        ServerConfig config
    ) {
        _cache = cache;
        _config = config;
    }

    public string CreateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task StoreRefreshToken(Guid userId, string refreshToken)
    {
        UserAuthData user = new() {
            Id = userId,
            RefreshToken = refreshToken,
            RefreshTokenExpiryTime = DateTime.Now.AddHours(1)
        };
        await _cache.SetStringAsync(userId.ToString(), JsonSerializer.Serialize(user));
    }

    public async Task<string> NewAccessToken(User user, string refreshToken)
    {
        var json = await _cache.GetStringAsync(user.Id.ToString());
        if (json is null || json.Length == 0)
            throw new KeyNotFoundException($"Not found key ${user.Id} in the cache");
        var userAuthData = JsonSerializer.Deserialize<UserAuthData>(json);
        if (userAuthData is null)
            throw new Exception($"On deserializing cache entry returned an null reference. cache entry: {json}. user: {user.Id}");
        if (userAuthData.RefreshToken != refreshToken)
            throw new Exception($"refresh token {userAuthData.RefreshToken} registered to user {user.Id} is different than {refreshToken}");
        if (userAuthData.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new SecurityTokenExpiredException($"refresh token to user {user.Id} expired {userAuthData.RefreshTokenExpiryTime}");
        return CreateAccessToken(user);
    }

    public async Task<UserAuthData> GetUserAuthenticated(Guid userId)
    {
        var json = await _cache.GetStringAsync(userId.ToString());
        if (json is null || json.Length == 0)
            throw new KeyNotFoundException($"Not found key {userId} in the cache");
        var user = JsonSerializer.Deserialize<UserAuthData>(json);
        if (user is null)
            throw new Exception($"On deserializing cache entry returned an null reference. cache entry: {json}. userId: {userId}");
        return user;
    }

    public string CreateAccessToken(User user)
    {
        List<Claim> claims = new() { 
            new(ClaimTypes.Sid, user.Id.ToString()),
        };
        if (user.Email is not null)
            claims.Add(new(ClaimTypes.Email, user.Email));
        if (user.Name is not null)
            claims.Add(new(ClaimTypes.Name, user.Name));
        if (user.Roles.Count > 0) {
            foreach (var userRole in user.Roles)
                claims.Add(new(ClaimTypes.Role, userRole.RoleId.ToString()));
        }
        return new JwtSecurityTokenHandler().WriteToken(JwtToken(claims));
    }

    JwtSecurityToken JwtToken(List<Claim> claims) => new JwtSecurityToken(
        _config.Issuer, 
        _config.Audience, 
        claims, 
        expires: DateTime.UtcNow
            .AddSeconds(_config.SecondsAuthTokenExpire), 
        signingCredentials: GetSigningCredentials()
    );

    SigningCredentials GetSigningCredentials() => new SigningCredentials(
        GetSecurityKey(), 
        SecurityAlgorithms.HmacSha256Signature
    );

    SymmetricSecurityKey GetSecurityKey() => 
        new SymmetricSecurityKey(_config.SecretKey);

    public Guid GetUsernameFromAccessToken(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        if (!tokenHandler.CanReadToken(accessToken))
            throw new Exception($"Not possible read token {accessToken}");
        var validationParameters = GetValidationParameters();
        ClaimsPrincipal principal = tokenHandler.ValidateToken(
            accessToken, 
            validationParameters, 
            out SecurityToken token
        );
        Claim? claim = principal
            .FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
        if (claim is null)
            throw new Exception($"{ClaimTypes.NameIdentifier} is null on token {accessToken}");
        return Guid.Parse(claim.Value);
    }

    TokenValidationParameters GetValidationParameters() =>
        new TokenValidationParameters() {
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateAudience = true,
            ValidAudience = _config.Audience,
            ValidIssuer = _config.Issuer,
            IssuerSigningKey = GetSecurityKey()
        };

    public User GetUser(IEnumerable<Claim> claims) {
        User user = new();
        foreach(var claim in claims)
        {
            if(claim.Type == ClaimTypes.Sid)
                user.Id = Guid.Parse(claim.Value);
            else if (claim.Type == ClaimTypes.Email)
                user.Email = claim.Value;
            else if (claim.Type == ClaimTypes.Name)
                user.Name = claim.Value;
        }
        return user;
    }
}