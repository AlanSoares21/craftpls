using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.ViewsComponents;

public class SearchBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string? value = null, string? hidelabel = "n")
    {
        ViewData["CraftItemSearched"] = value;
        ViewData["HideSearchLabel"] = hidelabel == "y";
        return View();
    }
}