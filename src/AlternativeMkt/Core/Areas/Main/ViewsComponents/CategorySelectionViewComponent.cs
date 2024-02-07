using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.ViewsComponents;

public class CategorySelectionViewComponent: ViewComponent
{
    readonly MktDbContext _db;
    public CategorySelectionViewComponent(MktDbContext db) {
        _db = db;
    }
    public IViewComponentResult Invoke(string name, string hasoptionany = "n", int selected = -1)
    {
        return View(new CategorySelectionData() {
            Name = name,
            HasOptionAny = hasoptionany == "y",
            Categories = _db.CraftCategories.ToList(),
            CategorySelected = selected
        });
    }
}