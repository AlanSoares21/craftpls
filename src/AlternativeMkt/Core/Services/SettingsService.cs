using System.Text.Json;
using AlternativeMkt.Db;
using Microsoft.Extensions.Caching.Distributed;

namespace AlternativeMkt.Services;

public class SettingsService: ISettingsService
{
    const string settingsKey = "Settings";
    MktDbContext _db; 
    IDistributedCache _cache;
    public SettingsService(MktDbContext db, IDistributedCache cache) {
        _db = db;
        _cache = cache;
    }
    public async Task<Settings> GetSettings() {
        Settings? settings = null;
        string? settingsAsJson = await _cache.GetStringAsync(settingsKey);
        if (settingsAsJson is not null) {    
            settings = JsonSerializer.Deserialize<Settings>(settingsAsJson);
        }
        if (settings is null) {
            settings = await GetSettingsFromDb();
            await _cache.SetStringAsync(
                settingsKey, 
                JsonSerializer.Serialize(settings)
            );
        }
        return settings;
    }

    async Task<Settings> GetSettingsFromDb() {
        Settings? settings = _db.Settings.FirstOrDefault();
        if (settings is null) {
            settings = new Settings() {
                AboutCraftPls = "About craft pls",
                AboutInstance = "About this instance",
                AboutInstanceShort = "Short description about this instance"
            };
            await _db.Settings.AddAsync(settings);
            await _db.SaveChangesAsync();
        }
        return settings;
    }
    public async Task<bool> UpdateSettings(Settings settings) {
        _db.Settings.Update(settings);
        int rowsAffected = await _db.SaveChangesAsync();
        await _cache.RemoveAsync(settingsKey);
        return rowsAffected == 1;
    }
}