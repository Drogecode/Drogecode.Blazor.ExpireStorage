namespace Drogecode.Blazor.ExpireStorage.Models;

public class ExpiryStorageModel<T>
{
    public long Ttl { get; set; }
    public T Data { get; set; }
}
