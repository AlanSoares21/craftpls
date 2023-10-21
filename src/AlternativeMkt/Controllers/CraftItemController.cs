
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Controllers;

public class CraftItemController: BaseController
{
    private MktDbContext _db;
    public CraftItemController(MktDbContext db) {
        _db = db;
    }
    public async Task<IActionResult> Search(string searchTerm = "") {
        return View(
            _db.CraftItems.Where(i => 
                i.Name != null 
                && i.Name.Contains(searchTerm)
            ).ToList()
        );
    }
}