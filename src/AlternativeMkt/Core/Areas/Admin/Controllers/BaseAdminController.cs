
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AlternativeMkt.Admin.Controllers;

[Area("admin")]
[Authorize(Policy = "AdminAccess")]
public abstract class BaseAdminController : Controller
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