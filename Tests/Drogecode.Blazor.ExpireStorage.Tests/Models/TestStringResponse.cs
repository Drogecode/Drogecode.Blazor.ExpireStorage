namespace Drogecode.Blazor.ExpireStorage.Tests.Models;

public class TestStringResponse : ICacheableResponse
{
    public HandledBy HandledBy { get; set; }
    public string? Data { get; set; }
}