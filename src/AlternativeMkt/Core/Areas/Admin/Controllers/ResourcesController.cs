
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Admin.Controllers;

public class ResourcesController : BaseAdminController
{   
    public IActionResult Index() => View();
}