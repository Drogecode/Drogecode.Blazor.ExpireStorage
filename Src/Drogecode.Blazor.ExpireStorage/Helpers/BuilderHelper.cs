using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Drogecode.Blazor.ExpireStorage.Interfaces;
using Drogecode.Blazor.ExpireStorage.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Drogecode.Blazor.ExpireStorage.Helpers;

public static class BuilderHelper
{
    public static IServiceCollection AddExpireStorage(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorage();
        services.AddBlazoredSessionStorage();
        services.TryAddScoped<ILocalStorageExpireService, LocalStorageExpireService>();
        services.TryAddScoped<ISessionExpireService, SessionExpireService>();
        services.TryAddScoped<IExpireStorageService, ExpireStorageService>();
        return services;
    }
}