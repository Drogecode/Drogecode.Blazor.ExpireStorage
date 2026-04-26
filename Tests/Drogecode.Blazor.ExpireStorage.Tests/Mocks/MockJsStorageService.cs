using Microsoft.Extensions.Caching.Memory;

namespace Drogecode.Blazor.ExpireStorage.Tests.Mocks;

public class MockJsStorageService : IJsStorageService
{
    private readonly IMemoryCache _memoryCache;

    public MockJsStorageService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T RetrieveItem<T>(string storageKey, StorageLocation storageLocation, T defaultIfNull)
    {
        if (_memoryCache.TryGetValue(storageKey + storageLocation, out object? value))
        {
            if (value is null) return defaultIfNull;
            try
            {
                return (T)value;
            }
            catch
            {
                return defaultIfNull;
            }
        }

        return defaultIfNull;
    }

    public async Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore, CancellationToken clt = default)
    {
        _memoryCache.Set(storageKey + storageLocation, itemToStore);
    }

    public async Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation, CancellationToken clt = default)
    {
        if (_memoryCache.TryGetValue(storageKey + storageLocation, out object? value))
        {
            if (value is null) return default;
            try
            {
                return (T)value;
            }
            catch
            {
                return default;
            }
        }

        return default;
    }

    public async Task RemoveItem(string storageKey, StorageLocation storageLocation, CancellationToken clt = default)
    {
        _memoryCache.Remove(storageKey + storageLocation);
    }
}