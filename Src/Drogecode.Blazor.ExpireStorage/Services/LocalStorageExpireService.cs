using System.Text.Json;
using Blazored.LocalStorage;
using Drogecode.Blazor.ExpireStorage.Helpers;
using Drogecode.Blazor.ExpireStorage.Models;
using Microsoft.JSInterop;

namespace Drogecode.Blazor.ExpireStorage;

public class LocalStorageExpireService : ILocalStorageExpireService
{
    private readonly ILocalStorageService _localStorageService;
    private readonly IJSRuntime _jsRuntime;
    private Lazy<IJSObjectReference> _accessorJsRef = new();

    public LocalStorageExpireService(ILocalStorageService localStorageService, IJSRuntime jsRuntime)
    {
        _localStorageService = localStorageService;
        _jsRuntime = jsRuntime;

        //Fire and forget
        Task.Run(DeleteExpiredCache);
    }

    private async Task DeleteExpiredCache()
    {
        try
        {
            //Wait 60 seconds before starting to delete, do not slow down page loading for cleanup.
            for (var i = 0; i < 60; i++)
            {
                await Task.Delay(1000);
            }
            var ttl = DateTime.UtcNow.Ticks;
            if (_accessorJsRef.IsValueCreated is false)
            {
                _accessorJsRef = new Lazy<IJSObjectReference>(await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/Drogecode.Blazor.ExpireStorage/js/LocalStorageAccessor.js"));
            }

            var packages = await _accessorJsRef.Value.InvokeAsync<Dictionary<string, string>?>("getAll");
            if (packages is null)
            {
                ConsoleHelper.WriteLine("No packages found");
                return;
            }
            ConsoleHelper.WriteLine($"Found {packages.Count} packages, current tick {ttl}");
            foreach (var package in packages)
            {
                ExpiryStorageModel<object>? expiryStorageModel = null;
                try
                {
                    expiryStorageModel = JsonSerializer.Deserialize<ExpiryStorageModel<object>>(package.Value);
                    if (expiryStorageModel == null || expiryStorageModel.Ttl == 0) continue;
                }
                catch (JsonException)
                {
                    // Ignore json exceptions
                    continue;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLine("Inner exception");
                    ConsoleHelper.WriteLine(ex);
                    continue;
                }

                if (expiryStorageModel.Ttl >= ttl) continue;
                ConsoleHelper.WriteLine($"localstorage deleting {package.Key}, expired {new DateTime(expiryStorageModel.Ttl)}");
                await _localStorageService.RemoveItemAsync(package.Key);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Outer exception");
            ConsoleHelper.WriteLine(ex);
        }
        finally
        {
            if (_accessorJsRef.IsValueCreated)
            {
                await _accessorJsRef.Value.DisposeAsync();
            }
        }
    }

    public async ValueTask<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _localStorageService.GetItemAsync<ExpiryStorageModel<T?>>(key, cancellationToken);
        if (value is null || value.Data is null)
            return default(T);
        if (value.Ttl < DateTime.UtcNow.Ticks)
        {
            ConsoleHelper.WriteLine($"localstorage deleting {key}, expired {new DateTime(value.Ttl)} on trying to get");
            await _localStorageService.RemoveItemAsync(key, cancellationToken);
            return default(T);
        }
        var result = value.Data;
        return result;
    }

    public async ValueTask SetItemAsync<T>(string key, T data, DateTime expire, CancellationToken cancellationToken = default)
    {
        var value = new ExpiryStorageModel<T>
        {
            Data = data,
            Ttl = expire.Ticks,
        };
        await _localStorageService.SetItemAsync(key, value, cancellationToken);
    }

    public async Task DeleteItemAsync(string key, CancellationToken clt)
    {
        await _localStorageService.RemoveItemAsync(key, clt);
    }
}
