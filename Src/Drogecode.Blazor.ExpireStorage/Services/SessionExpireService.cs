using Drogecode.Blazor.ExpireStorage.Models;

namespace Drogecode.Blazor.ExpireStorage;

public class SessionExpireService : ISessionExpireService
{
    private readonly IJsStorageService _jsStorageService;

    public SessionExpireService(IJsStorageService jsStorageService)
    {
        _jsStorageService = jsStorageService;
    }

    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken clt = default)
    {
        var value = await _jsStorageService.RetrieveItem<ExpiryStorageModel<T?>>(key, StorageLocation.BrowserSession, clt);
        var ttl = DateTime.UtcNow.Ticks;
        if (value is null || value.Data is null || value.Ttl <= ttl) return default;
        var result = value.Data;
        return result;
    }

    public async ValueTask SetItemAsync<T>(string key, T data, DateTime expire, CancellationToken clt = default)
    {
        var value = new ExpiryStorageModel<T>
        {
            Data = data,
            Ttl = expire.Ticks
        };
        await _jsStorageService.StoreItem(key, StorageLocation.BrowserSession, value, clt);
    }
}
