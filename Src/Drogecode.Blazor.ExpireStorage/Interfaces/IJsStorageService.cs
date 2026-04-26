namespace Drogecode.Blazor.ExpireStorage;

public interface IJsStorageService
{
    T RetrieveItem<T>(string storageKey, StorageLocation storage, T defaultIfNull);
    Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore);
    Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation);
    Task RemoveItem(string storageKey, StorageLocation storageLocation);
}