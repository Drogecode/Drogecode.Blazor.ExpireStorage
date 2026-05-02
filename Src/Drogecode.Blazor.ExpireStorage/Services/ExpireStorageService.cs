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

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly HashSet<string> _activeRequests = new();

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

            if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(10), clt))
            {
                ConsoleHelper.WriteLine("Timeout waiting for semaphore in CachedRequestAsync");
                return BuildResponse(defaultResponse, HandledBy.Default);
            }
            try
            {
                if (clt.IsCancellationRequested)
                {
                    return BuildResponse(defaultResponse, HandledBy.Default);
                }

                lock (_activeRequests)
                {
                    if (_activeRequests.Contains(cacheKey))
                    {
                        // Already requesting this key, let's avoid overlapping if it's not CachedAndReplace
                        // If it is CachedAndReplace, it's already being handled by Task.Run
                        if (!request.CachedAndReplace)
                        {
                            ConsoleHelper.WriteLine($"Already requesting {cacheKey}, skipping concurrent request");
                            return BuildResponse(defaultResponse, HandledBy.Default);
                        }
                    }
                }

                if (request.CachedAndReplace && !(IsOffline && request.CacheWhenOffline))
                {
                    var requestCopy = request;
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await RunSaveAndReturn(cacheKey, function, requestCopy, clt);
                        }
                        catch (Exception ex)
                        {
                            ConsoleHelper.WriteLine($"Error in background RunSaveAndReturn for {cacheKey}: {ex}");
                        }
                    }, clt);
                }

                if ((request.CachedAndReplace || request.OneCallPerSession || (IsOffline && request.CacheWhenOffline)) && !request.IgnoreCache)
                {
                    var sessionResult = await _sessionStorageExpireService.GetItemAsync<TRes>(cacheKey, clt);
                    if (sessionResult is not null)
                    {
                        return BuildResponse(sessionResult, HandledBy.Session);
                    }
                }

                if ((request.CachedAndReplace || request.OneCallPerLocalStorage || (IsOffline && request.CacheWhenOffline)) && !request.IgnoreCache)
                {
                    var storageResult = await _localStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
                    if (storageResult is not null)
                    {
                        return BuildResponse(storageResult, HandledBy.LocalStorage);
                    }
                }

                if (!request.CachedAndReplace)
                {
                    return await RunSaveAndReturn(cacheKey, function, request, clt);
                }

                var cacheResult = await _localStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
                if (cacheResult is not null)
                {
                    return BuildResponse(cacheResult, HandledBy.LocalStorage);
                }

                cacheResult = await _sessionStorageExpireService.GetItemAsync<TRes?>(cacheKey, clt);
                if (cacheResult is not null)
                {
                    return BuildResponse(cacheResult, HandledBy.Session);
                }

                cacheResult ??= defaultResponse ?? Activator.CreateInstance<TRes>();
                return BuildResponse(cacheResult, HandledBy.Default);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteLine("HttpRequestException for " + cacheKey);
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
        lock (_activeRequests)
        {
            if (!_activeRequests.Add(cacheKey))
            {
                // This shouldn't really happen with the semaphore, but for background tasks it might
                return BuildResponse(default(TRes), HandledBy.Default);
            }
        }
        try
        {
            var result = await function();
            await _localStorageExpireService.SetItemAsync(cacheKey, result, request.ExpireLocalStorage, clt);
            if (request.OneCallPerSession)
            {
                await _sessionStorageExpireService.SetItemAsync(cacheKey, result, request.ExpireSession, clt);
            }

            IsOffline = false;
            return BuildResponse(result, HandledBy.Function);
        }
        finally
        {
            lock (_activeRequests)
            {
                _activeRequests.Remove(cacheKey);
            }
        }
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