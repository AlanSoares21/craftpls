
using AlternativeMkt.Admin.Models;
using AlternativeMkt.Api.Models;
using AlternativeMkt.Db;

namespace AlternativeMkt.Services;

public interface ICraftItemService
{
    StandardList<CraftItem> SearchItems(ListItemsParams query);
    Task<bool> Delete(int itemId);
}