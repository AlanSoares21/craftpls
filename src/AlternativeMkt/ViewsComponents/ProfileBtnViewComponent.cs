
using AlternativeMkt.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.ViewsComponents;

public class ProfileBtnViewComponent: ViewComponent
{
    IAuthService _auth;
    public ProfileBtnViewComponent(IAuthService auth) {
        _auth = auth;
    }
    public IViewComponentResult Invoke()
    {
        if (Request.Path.Value is not null && Request.Path.Value.Contains("Account"))
            return View("NoRender");

        if (User.Identity is not null && User.Identity.IsAuthenticated) {
            var authData = _auth.GetUser(UserClaimsPrincipal.Claims);
            if (authData.Name is not null)
                ViewData["Username"] = authData.Name;
            else if (authData.Email is not null)
                ViewData["Username"] = authData.Email;
            else
                ViewData["Username"] = authData.Id.ToString();
        }
        return View();
    }
}