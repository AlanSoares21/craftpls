using AlternativeMkt.Db;

namespace AlternativeMkt.Services;

public interface ISettingsService
{
    Task<Settings> GetSettings();
    Task<bool> UpdateSettings(Settings settings);
}