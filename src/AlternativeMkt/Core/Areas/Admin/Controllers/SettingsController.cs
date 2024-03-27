
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Db;
using AlternativeMkt.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Admin.Controllers;

public class SettingsController : BaseAdminController
{   
    ISettingsService _settingsService;
    ILogger<SettingsController> _logger;
    public SettingsController(
        ISettingsService settingsService,
        ILogger<SettingsController> logger
    ) {
        _settingsService = settingsService;
        _logger = logger;
    }
    public async Task<IActionResult> Index() => 
        View(await _settingsService.GetSettings());
    public async Task<IActionResult> Update(
        [Bind("aboutInstance,aboutInstanceShort,aboutCraftPls")] 
        UpdateSettings data
    ) {
        Settings settings = await _settingsService.GetSettings();
        settings.AboutCraftPls = data.aboutCraftPls;
        settings.AboutInstance = data.aboutInstance;
        settings.AboutInstanceShort = data.aboutInstanceShort;
        _logger.LogInformation("Updating settings");
        bool result = await _settingsService.UpdateSettings(settings);
        _logger.LogInformation("Settings updated. result: {result}", result);
        if (result)
            return RedirectToAction("Index");
        return View("Error", "Error on updating settings.");
    }
}