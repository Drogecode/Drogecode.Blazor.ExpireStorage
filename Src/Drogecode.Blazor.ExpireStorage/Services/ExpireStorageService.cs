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
        CancellationToken clt = default)
    {
        request ??= new CachedRequest();
        try
        {
            if (!string.IsNullOrEmpty(Postfix))
            {
                cacheKey += $"__{Postfix}";
            }
            if (clt.IsCancellationRequested) return default;
            if (request.CachedAndReplace)
            {
                var requestCopy = request;
                _ = Task.Run(async () => await RunSaveAndReturn(cacheKey, function, requestCopy, clt), clt);
            }

            if ((request.CachedAndReplace || request.OneCallPerSession) && !request.IgnoreCache)
            {
                var sessionResult = await _sessionStorageExpireService.GetItemAsync<TRes>(cacheKey, clt);
                if (sessionResult is not null)
                {
                    if (sessionResult is ICacheableResponse response)
                    {
                        response.FromCache = true;
                    }
                    return sessionResult;
                }
            }

            if ((request.CachedAndReplace || request.OneCallPerCache) && !request.IgnoreCache)
            {
                var cacheResult = await _localStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
                if (cacheResult is not null)
                {
                    if (cacheResult is ICacheableResponse response)
                    {
                        response.FromCache = true;
                    }
                    return cacheResult;
                }
            }

            if (!request.CachedAndReplace)
            {
                return await RunSaveAndReturn(cacheKey, function, request, clt);
            }
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteLine("a HttpRequestException");
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
            return default(TRes);
        }

        try
        {
            var cacheResult = await _localStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
            cacheResult ??= Activator.CreateInstance<TRes>();
            if (cacheResult is ICacheableResponse response)
            {
                response.FromCache = true;
            }

            return cacheResult;
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteLine("b HttpRequestException");
        }
        catch (TaskCanceledException)
        {
        }
        catch (JsonException)
        {
            // The object definition could be changed with an update. Deleting the old version and retrying again to get the latest version.
            ConsoleHelper.WriteLine($"JsonException for {cacheKey}, Deleting");
            await _localStorageExpireService.DeleteItemAsync(cacheKey, clt);
            request ??= new CachedRequest();
            if (request.RetryOnJsonException) // Only retry once
            {
                ConsoleHelper.WriteLine($"Retry calling {cacheKey}");
                request.RetryOnJsonException = false;
                return await CachedRequestAsync<TRes>(cacheKey, function, request, clt);
            }

            ConsoleHelper.WriteLine($"Will not retry {cacheKey}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine(ex);
        }

        return default(TRes);
    }

    private async Task<TRes> RunSaveAndReturn<TRes>(string cacheKey, Func<Task<TRes>> function, CachedRequest request, CancellationToken clt)
    {
        var result = await function();
        await _localStorageExpireService.SetItemAsync(cacheKey, result, request.ExpireLocalStorage, clt);
        if (request.OneCallPerSession)
            await _sessionStorageExpireService.SetItemAsync(cacheKey, result, request.ExpireSession, clt);
        return result;
    }
}