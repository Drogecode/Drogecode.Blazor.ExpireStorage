namespace Drogecode.Blazor.ExpireStorage.Interfaces;

public interface ILocalStorageExpireService
{
    ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default);
    ValueTask SetItemAsync<T>(string key, T data, DateTime expire, CancellationToken cancellationToken = default);
    Task DeleteItemAsync(string key, CancellationToken clt);
}
