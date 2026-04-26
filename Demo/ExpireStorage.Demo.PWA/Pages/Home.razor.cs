using System.Diagnostics.CodeAnalysis;
using Drogecode.Blazor.ExpireStorage;
using ExpireStorage.Demo.PWA.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ExpireStorage.Demo.PWA.Pages;

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

    private async Task Save()
    {
        _cachedRequest.ExpireLocalStorage = DateTime.UtcNow.AddDays(_storageSettings.LocalStorageDaysInFuture);
        _cachedRequest.ExpireSession = DateTime.UtcNow.AddMinutes(_storageSettings.SessionStorageMinutesInFuture);

        var value = await StorageService.CachedRequestAsync(_storageSettings.Key,
            async () => await FunctionToCall(),
            _cachedRequest,
            new DemoModelForStorage(),
            _cls.Token);
        _response = value?.Data ?? "No data";
        _handledBy = value?.HandledBy ?? HandledBy.None;
    }

    // This function could be a call to a server side API.
    private async Task<DemoModelForStorage> FunctionToCall()
    {
        return _model;
    }

    public void Dispose()
    {
        _cls.Cancel();
        // Do not use _cls.Dispose() because it could throw an exception.
    }
}