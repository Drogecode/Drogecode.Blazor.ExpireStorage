using System.Text.Json;
using Drogecode.Blazor.ExpireStorage.Helpers;
using Microsoft.JSInterop;

namespace Drogecode.Blazor.ExpireStorage;

internal class JsStorageService : IJsStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, string> _pageCache = new();

    public JsStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public T RetrieveItem<T>(string storageKey, StorageLocation storage, T defaultIfNull)
    {
        try
        {
            var stringFromCache = storage switch
            {
                StorageLocation.BrowserLocal => ((IJSInProcessRuntime)_jsRuntime).Invoke<string?>("localStorage.getItem", storageKey) ?? string.Empty,
                StorageLocation.BrowserSession => ((IJSInProcessRuntime)_jsRuntime).Invoke<string?>("sessionStorage.getItem", storageKey) ?? string.Empty,
                _ => _pageCache[storageKey]
            };
            if (string.IsNullOrEmpty(stringFromCache)) return defaultIfNull;

            return JsonSerializer.Deserialize<T>(stringFromCache) ?? defaultIfNull;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Exception in RetrieveItem", ex);
            return defaultIfNull;
        }
    }

    public async Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation, CancellationToken clt = default)
    {
        try
        {
            var stringFromCache = storageLocation switch
            {
                StorageLocation.BrowserLocal => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", clt, storageKey) ?? string.Empty,
                StorageLocation.BrowserSession => await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", clt, storageKey) ?? string.Empty,
                _ => _pageCache.TryGetValue(storageKey, out string? cachedItem) ? cachedItem : string.Empty
            };

            if (string.IsNullOrEmpty(stringFromCache)) return default;

            return JsonSerializer.Deserialize<T>(stringFromCache) ?? default;
        }

        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Exception in RetrieveItem (async)", ex);
            return default;
        }
    }

    public async Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore, CancellationToken clt = default)
    {
        try
        {
            var asString = JsonSerializer.Serialize(itemToStore);

            switch (storageLocation)
            {
                case StorageLocation.BrowserLocal:
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", clt, storageKey, asString);
                    break;
                case StorageLocation.BrowserSession:
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", clt, storageKey, asString);
                    break;
                default:
                    _pageCache[storageKey] = asString;
                    break;
            }
        }

        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Exception in StoreItem", ex);
        }
    }

    public async Task RemoveItem(string storageKey, StorageLocation storageLocation, CancellationToken clt = default)
    {
        try
        {
            switch (storageLocation)
            {
                case StorageLocation.BrowserLocal:
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", clt, storageKey);
                    break;
                case StorageLocation.BrowserSession:
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", clt, storageKey);
                    break;
                default:
                    _pageCache.Remove(storageKey);
                    break;
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Exception in RemoveItem", ex);
        }
    }
}
