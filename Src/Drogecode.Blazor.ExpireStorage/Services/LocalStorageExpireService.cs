using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Drogecode.Blazor.ExpireStorage.Enums;
using Drogecode.Blazor.ExpireStorage.Helpers;
using Drogecode.Blazor.ExpireStorage.Interfaces;
using Drogecode.Blazor.ExpireStorage.Models;
using Microsoft.JSInterop;

namespace Drogecode.Blazor.ExpireStorage;

public class LocalStorageExpireService : ILocalStorageExpireService
{
    private readonly IExpireStorageJsService _expireStorageJsService;
    private readonly IJSRuntime _jsRuntime;
    private Lazy<IJSObjectReference> _accessorJsRef = new();

    public LocalStorageExpireService(IExpireStorageJsService expireStorageJsService, IJSRuntime jsRuntime)
    {
        _expireStorageJsService = expireStorageJsService;
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
            var count = 0;
            foreach (var package in packages)
            {
                ExpiryStorageModel<object>? expiryStorageModel = null;
                try
                {
                    if (Base64.IsValid(package.Value))
                    {
                        var utf8Byes = Convert.FromBase64String(package.Value);
                        var jsonString = Encoding.UTF8.GetString(utf8Byes);
                        expiryStorageModel = JsonSerializer.Deserialize<ExpiryStorageModel<object>>(jsonString);
                    }
                    else
                    {
                        expiryStorageModel = JsonSerializer.Deserialize<ExpiryStorageModel<object>>(package.Value);
                    }

                    if (expiryStorageModel == null || expiryStorageModel.Ttl == 0) continue;
                }
                catch (JsonException)
                {
                    // Ignore json exceptions
                    continue;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteLine("Inner exception", ex);
                    continue;
                }

                if (expiryStorageModel.Ttl >= ttl) continue;
                ConsoleHelper.WriteLine($"localstorage deleting {package.Key}, expired {new DateTime(expiryStorageModel.Ttl)}");
                await _expireStorageJsService.RemoveItem(package.Key, StorageLocation.BrowserLocal);
                count++;
            }

            ConsoleHelper.WriteLine($"{count}/{packages.Count} items where expired and deleted.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteLine("Outer exception", ex);
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
        var value = await _expireStorageJsService.RetrieveItem<ExpiryStorageModel<T?>>(key, StorageLocation.BrowserLocal);
        if (value is null || value.Data is null)
            return default(T);
        if (value.Ttl < DateTime.UtcNow.Ticks)
        {
            ConsoleHelper.WriteLine($"localstorage deleting {key}, expired {new DateTime(value.Ttl)} on trying to get");
            await _expireStorageJsService.RemoveItem(key, StorageLocation.BrowserLocal);
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
        await _expireStorageJsService.StoreItem(key, StorageLocation.BrowserLocal, value);
    }

    public async Task DeleteItemAsync(string key, CancellationToken clt)
    {
        await _expireStorageJsService.RemoveItem(key, StorageLocation.BrowserLocal);
    }
}