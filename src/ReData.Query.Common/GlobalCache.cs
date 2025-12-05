using Microsoft.Extensions.Caching.Memory;

namespace ReData.Query.Common;

public class Global
{
    public static MemoryCache MemoryCache { get; } = new(new MemoryCacheOptions());
}