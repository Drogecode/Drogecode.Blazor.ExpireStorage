using Blazored.SessionStorage;
using Microsoft.Extensions.Caching.Memory;

namespace Drogecode.Blazor.ExpireStorage.Tests.Mocks;

public class SessionStorageServiceMock : ISessionStorageService
{
    private readonly IMemoryCache _memoryCache;

    public SessionStorageServiceMock(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async ValueTask ClearAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        if (_memoryCache.TryGetValue($"session_{key}", out object? value))
        {
            if (value is null) return default;
            // The value in cache is likely ExpiryStorageModel<T>
            try { return (T)value; } catch { return default; }
        }
        return default;
    }

    public async ValueTask<string?> GetItemAsStringAsync(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<string?> KeyAsync(int index, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<IEnumerable<string>> KeysAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask<int> LengthAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = new CancellationToken())
    {
        _memoryCache.Remove($"session_{key}");
    }

    public async ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public async ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = new CancellationToken())
    {
        System.Console.WriteLine($"[DEBUG_LOG] SessionStorage SetItem: {key} type: {typeof(T)}");
        _memoryCache.Set($"session_{key}", data);
    }

    public async ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public event EventHandler<ChangingEventArgs>? Changing;
    public event EventHandler<ChangedEventArgs>? Changed;
}