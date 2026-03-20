using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Drogecode.Blazor.ExpireStorage.Helpers;

namespace Drogecode.Blazor.ExpireStorage;

public class ExpireStorageService : IExpireStorageService
{
    private readonly ILocalStorageExpireService _localStorageExpireService;
    private readonly ISessionExpireService _sessionStorageExpireService;

    /// <summary>
    /// String to postfix to the key in case multiple users can use the app from the same browser.
    /// </summary>
    public static string? Postfix { get; set; }

    public static event Func<bool, Task>? IsOfflineChanged;

    /// <summary>
    /// True if the last request failed.
    /// </summary>
    public static bool IsOffline
    {
        get => field;
        private set
        {
            if (field == value) return;
            field = value;
            IsOfflineChanged?.Invoke(field);
        }
    }

    public static bool LogToConsole
    {
        get => ConsoleHelper.LogToConsole;
        set => ConsoleHelper.LogToConsole = value;
    }

    public ExpireStorageService(
        ILocalStorageExpireService localStorageExpireService,
        ISessionExpireService sessionStorageExpireService)
    {
        _localStorageExpireService = localStorageExpireService;
        _sessionStorageExpireService = sessionStorageExpireService;
    }

    
    public async Task<TRes?> CachedRequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRes>(
        string cacheKey,
        Func<Task<TRes>> function,
        CachedRequest? request = null,
        TRes? defaultResponse = default,
        CancellationToken clt = default)
    {
        request ??= new CachedRequest();
        try
        {
            if (!string.IsNullOrEmpty(Postfix))
            {
                cacheKey += $"__{Postfix}";
            }

            if (clt.IsCancellationRequested)
            {
                return BuildResponse(defaultResponse, HandledBy.Default);
            }
            if (request.CachedAndReplace && !(IsOffline && request.AlwaysCacheWhenOffline))
            {
                var requestCopy = request;
                _ = Task.Run(async () => await RunSaveAndReturn(cacheKey, function, requestCopy, clt), clt);
            }

            if ((request.CachedAndReplace || request.OneCallPerSession || (IsOffline && request.AlwaysCacheWhenOffline)) && !request.IgnoreCache)
            {
                var sessionResult = await _sessionStorageExpireService.GetItemAsync<TRes>(cacheKey, clt);
                return BuildResponse(sessionResult, HandledBy.Cache);
            }

            if ((request.CachedAndReplace || request.OneCallPerCache || (IsOffline && request.AlwaysCacheWhenOffline)) && !request.IgnoreCache)
            {
                var cacheResult = await _localStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
                return BuildResponse(cacheResult, HandledBy.Cache);
            }

            if (!request.CachedAndReplace)
            {
                return await RunSaveAndReturn(cacheKey, function, request, clt);
            }
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteLine("a HttpRequestException");
            IsOffline = true;
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine(ex);
        }

        if (request.IgnoreCache)
        {
            return BuildResponse(defaultResponse, HandledBy.Default);
        }

        try
        {
            var cacheResult = await _localStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
            cacheResult ??= Activator.CreateInstance<TRes>();
            return BuildResponse(cacheResult, HandledBy.Cache);
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteLine("b HttpRequestException");
            IsOffline = true;
        }
        catch (TaskCanceledException)
        {
        }
        catch (JsonException)
        {
            // The object definition could be changed with an update. Deleting the old version and retrying again to get the latest version.
            ConsoleHelper.WriteLine($"JsonException for {cacheKey}, Deleting");
            await _localStorageExpireService.DeleteItemAsync(cacheKey, clt);
            if (request.RetryOnJsonException) // Only retry once
            {
                ConsoleHelper.WriteLine($"Retry calling {cacheKey}");
                request.RetryOnJsonException = false;
                return await CachedRequestAsync(cacheKey, function, request, defaultResponse, clt);
            }

            ConsoleHelper.WriteLine($"Will not retry {cacheKey}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine(ex);
        }

        return BuildResponse(defaultResponse, HandledBy.Default);
    }

    private async Task<TRes?> RunSaveAndReturn<TRes>(string cacheKey, Func<Task<TRes>> function, CachedRequest request, CancellationToken clt)
    {
        var result = await function();
        await _localStorageExpireService.SetItemAsync(cacheKey, result, request.ExpireLocalStorage, clt);
        if (request.OneCallPerSession)
            await _sessionStorageExpireService.SetItemAsync(cacheKey, result, request.ExpireSession, clt);
        IsOffline = false;
        return BuildResponse(result, HandledBy.Function);
    }

    private static TRes? BuildResponse<TRes>(TRes? result, HandledBy handledBy)
    {
        if (result is null) return result;
        if (result is ICacheableResponse response)
        {
            response.HandledBy = handledBy;
        }
        return result;
    }
}