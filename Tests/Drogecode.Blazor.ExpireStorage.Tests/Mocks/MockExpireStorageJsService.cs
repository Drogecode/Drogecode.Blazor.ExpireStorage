using Drogecode.Blazor.ExpireStorage.Enums;
using Drogecode.Blazor.ExpireStorage.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Drogecode.Blazor.ExpireStorage.Tests.Mocks;

public class MockExpireStorageJsService : IExpireStorageJsService
{
    private readonly IMemoryCache _memoryCache;
    public MockExpireStorageJsService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }
    
    public T RetrieveItem<T>(string storageKey, StorageLocation storage, T defaultIfNull) where T : notnull
    {
        if (_memoryCache.TryGetValue(storageKey, out object? value))
        {
            if (value is null) return defaultIfNull;
            try { return (T)value; } catch { return defaultIfNull; }
        }
        return defaultIfNull;
    }

    public async Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore) where T : notnull
    {
        _memoryCache.Set(storageKey, itemToStore);
    }

    public async Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation)
    {
        if (_memoryCache.TryGetValue(storageKey, out object? value))
        {
            if (value is null) return default;
            try { return (T)value; } catch { return default; }
        }
        return default;
    }

    public async Task RemoveItem(string storageKey, StorageLocation storageLocation)
    {
        _memoryCache.Remove(storageKey);
    }
}