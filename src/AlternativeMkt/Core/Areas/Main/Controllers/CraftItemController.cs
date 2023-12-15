
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlternativeMkt.Main.Controllers;

public class CraftItemController: BaseController
{
    private MktDbContext _db;
    public CraftItemController(MktDbContext db) {
        _db = db;
    }
    public async Task<IActionResult> Search(string searchTerm = "") {
        ViewData["CraftItemSearched"] = searchTerm;
        return View(
            _db.CraftItems.Include("Asset").Where(i => 
                i.Name != null 
                && EF.Functions.ILike(
                    i.Name,
                    $"%{searchTerm}%"
                )
            ).ToList()
        );
    }
}