namespace ExpireStorage.Demo.PWA.Models;

public class StorageSettings
{
    public string Key { get; set; } = "StorageKey";
    public int LocalStorageDaysInFuture { get; set; } = 7;
    public int SessionStorageMinutesInFuture { get; set; } = 10;
}