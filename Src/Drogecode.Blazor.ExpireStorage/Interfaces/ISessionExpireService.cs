namespace Drogecode.Blazor.ExpireStorage.Interfaces;

public interface ISessionExpireService
{
    ValueTask<T?> GetItemAsync<T>(string key, CancellationToken clt = default);
    ValueTask SetItemAsync<T>(string key, T data, DateTime expire, CancellationToken clt = default);
}
