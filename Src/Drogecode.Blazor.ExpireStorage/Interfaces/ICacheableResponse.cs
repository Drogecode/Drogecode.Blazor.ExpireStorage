namespace Drogecode.Blazor.ExpireStorage;

public interface ICacheableResponse
{
    public bool FromCache { get; set; }
}