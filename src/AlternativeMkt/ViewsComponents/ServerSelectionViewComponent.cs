using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.ViewsComponents;

public class ServerSelectionViewComponent: ViewComponent
{
    readonly MktDbContext _db;
    public ServerSelectionViewComponent(MktDbContext db) {
        _db = db;
    }
    public IViewComponentResult Invoke(string name)
    {
        ViewData["name"] = name;
        return View(_db.Servers.ToList());
    }
}