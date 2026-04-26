namespace ExpireStorage.Demo.PWA.Models;

public class StorageSettings
{
    public string Key { get; set; } = "StorageKey";
    public int LocalStorageDaysInFuture { get; set; } = 7;
    public int SessionStorageMinutesInFuture { get; set; } = 10;
    public int ResponseDelayInMs { get; set; } = 0;
    public bool MockOffline { get; set; } = false;
}