
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
        string? culture = context.RouteData.Values["culture"]?.ToString();
        if (string.IsNullOrEmpty(culture))
            culture = "en";
        var cultureInfo = CultureInfo.GetCultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        
        base.OnActionExecuting(context);
    }
}