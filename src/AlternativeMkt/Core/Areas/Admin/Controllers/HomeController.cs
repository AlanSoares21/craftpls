
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Admin.Controllers;

public class HomeController : BaseAdminController
{
    MktDbContext _db;
    public HomeController(MktDbContext db) {
        _db = db;
    }
    public IActionResult Index([FromQuery] StandardPaginationParams query)
    {
        if (query.start < 0)
            query.start = 0;
        if (query.count < 0 || query.count > 10)
            query.count = 10;
        StandardList<CreateUserAccountRequest> list = new() {
            Data = _db.CreateUserAccountRequests
                .OrderByDescending(r => r.CreatedAt)
                .Skip(query.start)
                .Take(query.count)
                .ToList(),
            Start = query.start,
            Count = query.count,
            Total = _db.CreateUserAccountRequests.Count()
        };
        return View(new ListAccountsRequestData() {
            Query = query,
            Requests = list
        }); 
    }
}