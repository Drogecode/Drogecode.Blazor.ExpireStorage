namespace Drogecode.Blazor.ExpireStorage;

public interface IJsStorageService
{
    T RetrieveItem<T>(string storageKey, StorageLocation storage, T defaultIfNull);
    Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation, CancellationToken clt = default);
    Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore, CancellationToken clt = default);
    Task RemoveItem(string storageKey, StorageLocation storageLocation, CancellationToken clt = default);
}