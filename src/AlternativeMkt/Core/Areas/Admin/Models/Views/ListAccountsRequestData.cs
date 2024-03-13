using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;

namespace AlternativeMkt.Admin.Models;

public class ListAccountsRequestData
{
    public StandardList<CreateUserAccountRequest> Requests { get; set; } = new();
    public StandardPaginationParams Query { get; set; } = new();
}