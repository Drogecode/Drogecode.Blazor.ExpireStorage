using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Drogecode.Blazor.ExpireStorage;

public static class BuilderHelper
{
    public static IServiceCollection AddExpireStorage(this IServiceCollection services)
    {
        services.TryAddScoped<ILocalStorageExpireService, LocalStorageExpireService>();
        services.TryAddScoped<ISessionExpireService, SessionExpireService>();
        services.TryAddScoped<IExpireStorageService, ExpireStorageService>();
        services.TryAddScoped<IJsStorageService, JsStorageService>();
        return services;
    }

    public static IServiceCollection AddExpireStorageAsSingleton(this IServiceCollection services)
    {
        services.TryAddSingleton<ILocalStorageExpireService, LocalStorageExpireService>();
        services.TryAddSingleton<ISessionExpireService, SessionExpireService>();
        services.TryAddSingleton<IExpireStorageService, ExpireStorageService>();
        services.TryAddSingleton<IJsStorageService, JsStorageService>();
        return services;
    }
}