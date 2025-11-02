using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Drogecode.Blazor.ExpireStorage;

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


    public static IServiceCollection AddExpireStorageAsSingleton(this IServiceCollection services)
    {
        services.AddBlazoredLocalStorageAsSingleton();
        services.AddBlazoredSessionStorageAsSingleton();
        services.TryAddSingleton<ILocalStorageExpireService, LocalStorageExpireService>();
        services.TryAddSingleton<ISessionExpireService, SessionExpireService>();
        services.TryAddSingleton<IExpireStorageService, ExpireStorageService>();
        return services;
    }
}