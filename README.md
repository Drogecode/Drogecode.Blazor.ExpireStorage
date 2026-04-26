[![Nuget version](https://img.shields.io/nuget/v/Drogecode.Blazor.ExpireStorage.svg?logo=nuget)](https://www.nuget.org/packages/Drogecode.Blazor.ExpireStorage/)

# Drogecode.Blazor.ExpireStorage

Store api responses in localstorage and sessionstorage.

Configure if the api should be called or the cached value will be returned if available.

## Installing

To install the package, add the following line to the csproj file. Replacing x.x.x with the latest version number (found at the top of this file):

```
<PackageReference Include="Drogecode.Blazor.ExpireStorage" Version="x.x.x" />
```

You can also install via the .NET CLI with the following command:

```
dotnet add package Drogecode.Blazor.ExpireStorage
```

If you're using Visual Studio you can also install via the built in NuGet package manager.

## Setup

You will need to register the expire storage services with the service collection in your *Startup.cs* file in Blazor Server.

```c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddExpireStorage();
}
``` 

Or in your *Program.cs* file in Blazor WebAssembly.

```c#
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("app");

    builder.Services.AddExpireStorage();

    await builder.Build().RunAsync();
}
```

## Usage (Blazor WebAssembly)

### Parameters

| Parameter                 | Required | Description                                                                              |
|---------------------------|----------|------------------------------------------------------------------------------------------|
| string cacheKey           | Required | The key to use for the cache.                                                            |
| Func<Task<TRes>> function | Required | The function to call.                                                                    |
| CachedRequest? request    | Optional | settings for the cache request (explained below).                                        |
| TRes? defaultResponse     | Optional | default value to return if the function failed or was not called and the cache is empty. |
| CancellationToken clt     | Optional | a cancellation token.                                                                    |

### Example

```c#
@inject Drogecode.Blazor.ExpireStorage.IExpireStorageService storageService

@code {
    
    public async Task<YourObjectResponse?> GetDayItemsAsync(DateRange dateRange, Guid userId, CancellationToken clt)
    {
        var cacheKey = "CACHE_KEY_HERE"
        var response = await storageService.CachedRequestAsync(cacheKey,
            async () => await apiClient.GetItemsAsync(),
            new CachedRequest{CachedAndReplace = true},
            new YourObjectResponse(),
            cancellationToken);
        return response;
    }

}
```

## Options

### CachedRequest

You can give optional settings to the CachedRequest object.

| Parameter              | Description                                                                                                                                                                               | Default    |
|------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------|
| OneCallPerLocalStorage | If true, the result will be returned from localstorage if it is not expired.                                                                                                              | false      |
| OneCallPerSession      | If true, the result will be returned from sessionstorage if it is not expired.                                                                                                            | false      |
| ExpireLocalStorage     | The DateTime the localstorage value will be expired.                                                                                                                                      | 7 days     |
| ExpireSessionStorage   | The DateTime the sessionstorage value will be expired.                                                                                                                                    | 15 minutes |
| IgnoreCache            | If true, never return a cached result.                                                                                                                                                    | false      |
| CachedAndReplace       | If true, The cached result will be returned and the cache will be refreshed for the next call. If no cache is found, the default or NULL value will be returned.                          | false      |
| CacheWhenOffline       | If true, the cached result will be returned when offline, except when IgnoreCache is true.                                                                                                | false      |
| RetryOnJsonException   | If true, If a JSON exception occurs, the cache will be cleared and the request will be retried once. This will minimize the effect if a breaking change was introduced in the JSON value. | true       |

### Global settings

#### Postfix

On, for example, MainLayout.razor.cs, you can set the Postfix to be used for all requests. This is useful if you have multiple users using the same app from the same browser.

`ExpireStorageService.Postfix = userId.ToString();`

#### IsOffline

ExpireStorageService knows two properties to monitor if the app is offline.

IsOffline is true when the last request had an `HttpRequestException`, after a successful request IsOffline will be false.

`ExpireStorageService.IsOffline` and `ExpireStorageService.IsOfflineChanged`

#### LogToConsole

ExpireStorageService can log to the console if you want to see what is happening, *default: false*.

`ExpireStorageService.LogToConsole = true;`

### ICacheableResponse

If a response object implements ICacheableResponse, the HandledBy property will be set to `HandledBy.Cache` if the result was retrieved from cache and to `HandledBy.Default` if the default provided by
the caller was used.

```c#
using Drogecode.Blazor.ExpireStorage;
public class YourObjectResponse : ICacheableResponse
{
    ...
    public HandledBy HandledBy { get; set; }
    ...
}
```

## Cache

The cache is stored as a base64 string and serialized / deserialized using System.Text.Json.

### Cleanup

One minute after the app starts, the local storage cache will be cleared from all expired values.

Items that are expired but not yet deleted will not be returned.
