
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Admin.Controllers;

public class ItemsController : BaseAdminController
{   
    public IActionResult Index() => View();
}