using System.Text;
using System.Text.Json;
using Drogecode.Blazor.ExpireStorage.Enums;
using Drogecode.Blazor.ExpireStorage.Helpers;
using Drogecode.Blazor.ExpireStorage.Interfaces;
using Microsoft.JSInterop;

namespace Drogecode.Blazor.ExpireStorage.Services;

internal class ExpireStorageJsService : IExpireStorageJsService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, string> _pageCache = new();

    public ExpireStorageJsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public T RetrieveItem<T>(string storageKey, StorageLocation storage, T defaultIfNull) where T : notnull
    {
        var base64String = storage switch
        {
            StorageLocation.BrowserLocal => ((IJSInProcessRuntime)_jsRuntime).Invoke<string?>("localStorage.getItem", storageKey) ?? string.Empty,
            StorageLocation.BrowserSession => ((IJSInProcessRuntime)_jsRuntime).Invoke<string?>("sessionStorage.getItem", storageKey) ?? string.Empty,
            _ => _pageCache[storageKey]
        };
        if (string.IsNullOrEmpty(base64String)) return defaultIfNull;
        var utf8Byes = Convert.FromBase64String(base64String);
        var jsonString = Encoding.UTF8.GetString(utf8Byes);

        if (string.IsNullOrEmpty(jsonString)) return defaultIfNull;

        return JsonSerializer.Deserialize<T>(jsonString) ?? defaultIfNull;
    }

    public async Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore) where T : notnull
    {
        var utf8Byes = JsonSerializer.SerializeToUtf8Bytes<T>(itemToStore);
        var base64String = Convert.ToBase64String(utf8Byes);

        switch (storageLocation)
        {
            case StorageLocation.BrowserLocal:
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, base64String);
                break;
            case StorageLocation.BrowserSession:
                await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", storageKey, base64String);
                break;
            default:
                _pageCache[storageKey] = base64String;
                break;
        }
    }

    public async Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation)
    {
        var base64String = storageLocation switch
        {
            StorageLocation.BrowserLocal => await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", storageKey) ?? string.Empty,
            StorageLocation.BrowserSession => await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", storageKey) ?? string.Empty,
            _ => _pageCache.TryGetValue(storageKey, out string? cachedItem) ? cachedItem : string.Empty
        };

        ConsoleHelper.WriteLine($"base64String: {base64String}");

        if (string.IsNullOrEmpty(base64String)) return default;
        var utf8Byes = Convert.FromBase64String(base64String);
        var jsonString = Encoding.UTF8.GetString(utf8Byes);

        return string.IsNullOrEmpty(jsonString) ? default : JsonSerializer.Deserialize<T>(jsonString);
    }


    public async Task RemoveItem(string storageKey, StorageLocation storageLocation)
    {
        switch (storageLocation)
        {
            case StorageLocation.BrowserLocal:
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", storageKey);
                break;
            case StorageLocation.BrowserSession:
                await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", storageKey);
                break;
            default:
                _pageCache.Remove(storageKey);
                break;
        }
    }
}