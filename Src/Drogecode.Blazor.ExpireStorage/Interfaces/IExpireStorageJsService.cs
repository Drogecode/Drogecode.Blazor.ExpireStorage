using Drogecode.Blazor.ExpireStorage.Enums;

namespace Drogecode.Blazor.ExpireStorage.Interfaces;

public interface IExpireStorageJsService
{
    T RetrieveItem<T>(string storageKey, StorageLocation storage, T defaultIfNull) where T : notnull;
    Task StoreItem<T>(string storageKey, StorageLocation storageLocation, T itemToStore) where T : notnull;
    Task<T?> RetrieveItem<T>(string storageKey, StorageLocation storageLocation);
    Task RemoveItem(string storageKey, StorageLocation storageLocation);
}