using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.ViewsComponents;

public class ServerSelectionViewComponent: ViewComponent
{
    readonly MktDbContext _db;
    public ServerSelectionViewComponent(MktDbContext db) {
        _db = db;
    }
    public IViewComponentResult Invoke(string name, string hasoptionany = "n", int selected = -1)
    {
        return View(new ServerSelectionData() {
            HasOptionAny = hasoptionany == "y",
            Name = name,
            Servers = _db.Servers.ToList(),
            ServerId = selected
        });
    }
}