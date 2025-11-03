using Blazored.SessionStorage;
using Drogecode.Blazor.ExpireStorage.Models;

namespace Drogecode.Blazor.ExpireStorage;

public class SessionExpireService : ISessionExpireService
{
    private readonly ISessionStorageService _sessionStorageService;

    public SessionExpireService(ISessionStorageService sessionStorageService)
    {
        _sessionStorageService = sessionStorageService;
    }

    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken clt = default)
    {
        var value = await _sessionStorageService.GetItemAsync<ExpiryStorageModel<T?>>(key, clt);
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
        await _sessionStorageService.SetItemAsync(key, value, clt);
    }
}
