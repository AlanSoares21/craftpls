
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Admin.Controllers;

public class HomeController : BaseAdminController
{
    public IActionResult Index()
    {
        return View();
    }
}