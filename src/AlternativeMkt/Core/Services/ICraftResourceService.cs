
using AlternativeMkt.Db;

namespace AlternativeMkt.Services;

public interface ICraftResourceService {
    Task AddResource(CraftResource resource);
    Task UpdateResource(CraftResource resource);
    Task DeleteResource(CraftResource resource);
}