using System.Security.Claims;
using AlternativeMkt.Db;

namespace AlternativeMkt.Auth;

public interface IAuthService
{
    string CreateAccessToken(User user, out DateTime expiresIn);
    string CreateRefreshToken();
    Task StoreRefreshToken(Guid userId, string refreshToken);
    User GetUserFromAccessToken(string accessToken);
    Task<UserAuthData> GetUserAuthenticated(Guid userId);
    User GetUser(IEnumerable<Claim> claims);
}