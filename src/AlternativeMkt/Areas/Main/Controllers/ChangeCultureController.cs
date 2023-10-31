
using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.Controllers;

public class ChangeCultureController: BaseController
{
    private ILogger<ChangeCultureController> _logger;
    public ChangeCultureController(ILogger<ChangeCultureController> logger) {
        _logger = logger;
    }
    public IActionResult Index(string culture, string redirectTo="") {
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        _logger.LogInformation("Changing culture to {c}", culture);
        if (!string.IsNullOrEmpty(redirectTo))
            return Redirect(redirectTo);
        return Redirect("Home");
    }

    public IActionResult RedirectToLocalized() {
        return RedirectPermanent("/main/en");
    }
}