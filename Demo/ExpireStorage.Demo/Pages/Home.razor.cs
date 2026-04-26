using System.Diagnostics.CodeAnalysis;
using Drogecode.Blazor.ExpireStorage;
using ExpireStorage.Demo.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ExpireStorage.Demo.Pages;

public sealed partial class Home : IDisposable
{
    [Inject, NotNull] private IExpireStorageService? StorageService { get; set; }

    private readonly CancellationTokenSource _cls = new();
    private DemoModelForStorage _model = new();
    private StorageSettings _storageSettings = new();
    private CachedRequest _cachedRequest = new();
    private MudForm _form;

    private string _response = string.Empty;
    private HandledBy _handledBy = HandledBy.None;
    private bool _isOffline = false;


    protected override void OnInitialized()
    {
        ExpireStorageService.LogToConsole = true;
        ExpireStorageService.IsOfflineChanged += OfflineChanged;
    }

    private async Task Save()
    {
        _response = string.Empty;
        _handledBy = HandledBy.None;
        StateHasChanged();
        _cachedRequest.ExpireLocalStorage = DateTime.UtcNow.AddDays(_storageSettings.LocalStorageDaysInFuture);
        _cachedRequest.ExpireSession = DateTime.UtcNow.AddMinutes(_storageSettings.SessionStorageMinutesInFuture);

        var value = await StorageService.CachedRequestAsync(_storageSettings.Key,
            async () => await FunctionToCall(),
            _cachedRequest,
            new DemoModelForStorage { Data = "Default object returned." },
            _cls.Token);
        _response = value?.Data ?? "No data";
        _handledBy = value?.HandledBy ?? HandledBy.None;
        StateHasChanged();
    }

    // This function could be a call to a server side API.
    private async Task<DemoModelForStorage> FunctionToCall()
    {
        if (_storageSettings.ResponseDelayInMs > 0)
        {
            await Task.Delay(_storageSettings.ResponseDelayInMs);
        }

        if (_storageSettings.MockOffline)
        {
            throw new HttpRequestException("Mock offline");
        }

        return _model;
    }

    private Task OfflineChanged(bool newValue)
    {
        _isOffline = newValue;
        StateHasChanged();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cls.Cancel();
        // Do not use _cls.Dispose() because it could throw an exception.
    }
}