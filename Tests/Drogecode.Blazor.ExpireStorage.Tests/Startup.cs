using Drogecode.Blazor.ExpireStorage.Interfaces;
using Drogecode.Blazor.ExpireStorage.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Xunit.DependencyInjection.Logging;

namespace Drogecode.Blazor.ExpireStorage.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ILocalStorageExpireService, LocalStorageExpireService>();
        services.AddScoped<ISessionExpireService, SessionExpireService>();
        services.AddScoped<IExpireStorageService, ExpireStorageService>();
        
        services.AddScoped<IJSRuntime, JSRuntimeMock>();
        services.AddScoped<IExpireStorageJsService, MockExpireStorageJsService>();

        services.AddLogging(lb => lb.AddXunitOutput());
    }
}