using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.ViewsComponents;

public class SearchBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string? value = null)
    {
        ViewData["CraftItemSearched"] = value;
        return View();
    }
}