
using AlternativeMkt.Db;

namespace AlternativeMkt.Services;

public interface ICraftResourceService {
    Task UpdateResource(CraftResource resource);
    Task DeleteResource(CraftResource resource);
}