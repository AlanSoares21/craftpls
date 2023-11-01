using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Api.Controllers;

[Area("api")]
[Route("api/{controller}")]
[ApiController]
public abstract class BaseApiController: ControllerBase
{
    
}