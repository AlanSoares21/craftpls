using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.Controllers;

public class LoginController : BaseController {
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() 
    {
        _logger.LogInformation("login index run");
        return View();
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginInput input) 
    {
        _logger.LogInformation("login post happen - email: {e}", input.email);
        return View(input);
    }
}