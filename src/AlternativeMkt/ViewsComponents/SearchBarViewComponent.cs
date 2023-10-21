using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.ViewsComponents;

public class SearchBarViewComponent : ViewComponent
{

    public IViewComponentResult Invoke()
    {
        return View();
    }
}