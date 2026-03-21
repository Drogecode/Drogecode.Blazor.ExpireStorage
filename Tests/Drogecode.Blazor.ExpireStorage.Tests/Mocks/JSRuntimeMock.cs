using Microsoft.JSInterop;

namespace Drogecode.Blazor.ExpireStorage.Tests.Mocks;

public class JSRuntimeMock : IJSRuntime
{
    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        throw new NotImplementedException();
    }
}