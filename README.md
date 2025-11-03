[![Nuget version](https://img.shields.io/nuget/v/Drogecode.Blazor.ExpireStorage.svg?logo=nuget)](https://www.nuget.org/packages/Drogecode.Blazor.ExpireStorage/)
[![Nuget downloads](https://img.shields.io/nuget/dt/Drogecode.Blazor.ExpireStorage?logo=nuget)](https://www.nuget.org/packages/Drogecode.Blazor.ExpireStorage/)

# Drogecode.Blazor.ExpireStorage
Adds a wrapper on top of [Blazored.LocalStorage](https://github.com/Blazored/LocalStorage) and [Blazored.SessionStorage](https://github.com/Blazored/SessionStorage) to expire items from localstorage and sessionstorage after a specified time.

## Installing

To install the package add the following line to you csproj file replacing x.x.x with the latest version number (found at the top of this file):

```
<PackageReference Include="Drogecode.Blazor.ExpireStorage" Version="x.x.x" />
```

You can also install via the .NET CLI with the following command:

```
dotnet add package Drogecode.Blazor.ExpireStorage
```

If you're using Visual Studio you can also install via the built in NuGet package manager.

## Setup

You will need to register the expire storage services with the service collection in your _Startup.cs_ file in Blazor Server.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddExpireStorage();
}
``` 

Or in your _Program.cs_ file in Blazor WebAssembly.

```c#
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("app");

    builder.Services.AddExpireStorage();

    await builder.Build().RunAsync();
}
```

If you use Blazored.LocalStorage or Blazored.SessionStorage with configuration those will need to be registered before Drogecode.Blazor.ExpireStorage.

### Registering services as Singleton - Blazor WebAssembly **ONLY**
99% of developers will want to register Blazored LocalStorage using the method described above. However, in some very specific scenarios
developer may have a need to register services as Singleton as apposed to Scoped. This is possible by using the following method:

```csharp
builder.Services.AddExpireStorageAsSingleton();
```

This method will not work with Blazor Server applications as Blazor's JS interop services are registered as Scoped and cannot be injected into Singletons.

## Usage (Blazor WebAssembly)
example

```c#
@inject Drogecode.Blazor.ExpireStorage.IExpireStorageService storageService

@code {
    
    public async Task<YourObjectResponse?> GetDayItemsAsync(DateRange dateRange, Guid userId, CancellationToken clt)
    {
        var cacheKey = "CACHE_KEY_HERE"
        var response = await storageService.CachedRequestAsync(cacheKey),
            async () => await apiClient.GetItemsAsync(),
            new CachedRequest{CachedAndReplace = true}, clt);
        return response;
    }

}
```

## Options

### CachedRequest
You can give optional settings to the CachedRequest object.

* OneCallPerSession - If true, the result will be cached in sessionstorage and not localstorage.
* OneCallPerCache - If true, the result will be cached in localstorage and not sessionstorage.
* IgnoreCache - If true, the result will never return a cached value.
* ExpireLocalStorage - The time to expire the result in localstorage. Default is 7 days.
* ExpireSessionStorage - The time to expire the result in sessionstorage. Default is 15 minutes.
* CachedAndReplace - If true, The cached result will be returned and the cache will be refreshed for the next call.
* RetryOnJsonException - If true, If a JSON exception occurs, the cache will be cleared and the request will be retried.

### Global settings

On, for example, MainLayout.razor.cs, you can set the Postfix to be used for all requests. This is useful if you have multiple users using the same app from the same browser.

`ExpireStorageService.Postfix = userId.ToString();`

### ICacheableResponse

If a response object implements ICacheableResponse, the FromCache property will be set to true if the result was retrieved from cache.

```c#
public class YourObjectResponse : ICacheableResponse
{
    ...
    public bool FromCache { get; set; }
    ...
}
```
