
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AlternativeMkt.Main.Controllers;

[Area("main")]
public abstract class BaseController : Controller
{
    public override void OnActionExecuting(
        ActionExecutingContext context
    ) {
        var cultureInfo = CultureInfo.GetCultureInfo(GetCulture());
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        base.OnActionExecuting(context);
    }

    protected string GetCulture() {
        string? culture = HttpContext.Request.Cookies["Culture"];
        if (string.IsNullOrEmpty(culture))
            return "en";
        return culture;
    }
}