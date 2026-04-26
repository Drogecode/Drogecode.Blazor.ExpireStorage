using System.Buffers.Text;
using System.Text;
using System.Text.Json;
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
        var stringFromCache = storage switch
        {
            StorageLocation.BrowserLocal => ((IJSInProcessRuntime)_jsRuntime).Invoke<string?>("localStorage.getItem", storageKey) ?? string.Empty,
            StorageLocation.BrowserSession => ((IJSInProcessRuntime)_jsRuntime).Invoke<string?>("sessionStorage.getItem", storageKey) ?? string.Empty,
            _ => _pageCache[storageKey]
        };
        if (string.IsNullOrEmpty(stringFromCache)) return defaultIfNull;

        string jsonString;
        if (Base64.IsValid(stringFromCache))
        {
            var utf8Byes = Convert.FromBase64String(stringFromCache);
            jsonString = Encoding.UTF8.GetString(utf8Byes);
        }
        else
        {
            jsonString = stringFromCache;
        }

        if (string.IsNullOrEmpty(jsonString)) return defaultIfNull;

        return JsonSerializer.Deserialize<T>(jsonString) ?? defaultIfNull;
    }

    public async Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation, CancellationToken clt = default)
    {
        var stringFromCache = storageLocation switch
        {
            StorageLocation.BrowserLocal => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", clt, storageKey) ?? string.Empty,
            StorageLocation.BrowserSession => await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", clt, storageKey) ?? string.Empty,
            _ => _pageCache.TryGetValue(storageKey, out string? cachedItem) ? cachedItem : string.Empty
        };
        string jsonString;
        if (Base64.IsValid(stringFromCache))
        {
            var utf8Byes = Convert.FromBase64String(stringFromCache);
            jsonString = Encoding.UTF8.GetString(utf8Byes);
        }
        else
        {
            jsonString = stringFromCache;
        }

        if (string.IsNullOrEmpty(jsonString)) return default;

        return JsonSerializer.Deserialize<T>(jsonString) ?? default;
    }

    public async Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore, CancellationToken clt = default)
    {
        var utf8Byes = JsonSerializer.SerializeToUtf8Bytes<T>(itemToStore);
        var base64String = Convert.ToBase64String(utf8Byes);

        switch (storageLocation)
        {
            case StorageLocation.BrowserLocal:
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", clt, storageKey, base64String);
                break;
            case StorageLocation.BrowserSession:
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", clt, storageKey, base64String);
                break;
            default:
                _pageCache[storageKey] = base64String;
                break;
        }
    }


    public async Task RemoveItem(string storageKey, StorageLocation storageLocation, CancellationToken clt = default)
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
}