using AlternativeMkt.Db;
using AlternativeMkt.Main.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlternativeMkt.Main.ViewsComponents;

public class PaginationViewComponent: ViewComponent
{
    public IViewComponentResult Invoke(
        int start,
        int count,
        int total,
        Dictionary<string, string> query)
    {
        if (count <= 0)
            count = 1;

        int last;
        if (total % count == 0)
            last = total - count;
        else
            last = (total / count) * count;

        return View(new PaginationData() {
            RouteParams = query,
            Count = count,
            CurrentIndex = start,
            NextIndex = start + count,
            PreviousIndex = start - count,
            CurrentPage = (start / count) + 1,
            LastValidIndex = last,
            Total = total
        });
    }
}