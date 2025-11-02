namespace Drogecode.Blazor.ExpireStorage;

internal class ExpiryStorageModel<T>
{
    public long Ttl { get; set; }
    public required T Data { get; set; }
}
