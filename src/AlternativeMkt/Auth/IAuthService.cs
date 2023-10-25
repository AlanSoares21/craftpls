using AlternativeMkt.Db;

namespace AlternativeMkt.Auth;

public interface IAuthService
{
    string CreateAccessToken(User user);
    string CreateRefreshToken();
    Task StoreRefreshToken(Guid userId, string refreshToken);
    Guid GetUsernameFromAccessToken(string accessToken);
    Task<UserAuthData> GetUserAuthenticated(Guid userId);
}