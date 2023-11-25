
namespace AlternativeMkt.Admin.Models;

public class SearchItemParams : PaginationParams
{
    public string? searchTerm { get; set; }
    public int? maxLevel { get; set; }
    public int? minLevel { get; set; }
}