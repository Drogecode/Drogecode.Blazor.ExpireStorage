using Drogecode.Blazor.ExpireStorage.Tests.Models;

namespace Drogecode.Blazor.ExpireStorage.Tests.Tests.Services;

public class ExpireStorageServiceTests : IDisposable
{
    private readonly IExpireStorageService _expireStorageService;

    public ExpireStorageServiceTests(IExpireStorageService expireStorageService)
    {
        _expireStorageService = expireStorageService;
        ExpireStorageService.LogToConsole = true;
    }

    public void Dispose()
    {
        ExpireStorageService.Postfix = null;
        ExpireStorageService.LogToConsole = false;
        // We can't easily reset IsOffline because it has a private setter and no public reset method, 
        // but we can trigger it to be false by a successful request.
    }

    [Fact]
    public async Task MinimalRequestTest()
    {
        const string cacheKey = "MinimalRequestTest";
        var response = await _expireStorageService.CachedRequestAsync<string>(cacheKey, () => Task.FromResult("test"), new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        response.Should().Be("test");
    }

    [Fact]
    public async Task ByFunctionTest()
    {
        const string cacheKey = "ByFunctionTest";
        var response = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
        {
            Data = "test"
        }), new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.HandledBy.Should().Be(HandledBy.Function);
        response.Data.Should().Be("test");
    }

    [Fact]
    public async Task FromCacheTest()
    {
        const string cacheKey = "FromCacheTest";
        var addToCache = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
            {
                Data = "test"
            }),
            new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) },
            clt: TestContext.Current.CancellationToken);
        Assert.NotNull(addToCache?.Data);
        addToCache.HandledBy.Should().Be(HandledBy.Function);
        var response = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
            {
                Data = "not called"
            }),
            new CachedRequest { OneCallPerCache = true, ExpireLocalStorage = DateTime.UtcNow.AddDays(7) },
            clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.HandledBy.Should().Be(HandledBy.Cache);
        response.Data.Should().Be("test");
    }

    [Fact]
    public async Task PostfixTest()
    {
        const string cacheKey = "PostfixTest";
        ExpireStorageService.Postfix = "user1";
        await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "data1" }), new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);

        ExpireStorageService.Postfix = "user2";
        var response2 = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "data2" }), new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);

        Assert.NotNull(response2?.Data);
        response2.Data.Should().Be("data2");
        response2.HandledBy.Should().Be(HandledBy.Function);

        ExpireStorageService.Postfix = "user1";
        var response1FromCache = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "not called" }), new CachedRequest { OneCallPerCache = true, ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response1FromCache?.Data);
        response1FromCache.Data.Should().Be("data1");
        response1FromCache.HandledBy.Should().Be(HandledBy.Cache);
    }

    [Fact]
    public async Task IgnoreCacheTest()
    {
        const string cacheKey = "IgnoreCacheTest";
        await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "cached" }), new CachedRequest { OneCallPerCache = true, ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);

        var response = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "fresh" }), new CachedRequest { IgnoreCache = true }, clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.Data.Should().Be("fresh");
        response.HandledBy.Should().Be(HandledBy.Function);
    }

    [Fact]
    public async Task HttpRequestExceptionSetsOfflineTest()
    {
        const string cacheKey = "OfflineTest";
        try
        {
            await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => throw new HttpRequestException(), new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);
        }
        catch (HttpRequestException) { }

        ExpireStorageService.IsOffline.Should().BeTrue();

        // Recover
        await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "recovered" }), new CachedRequest { ExpireLocalStorage = DateTime.UtcNow.AddDays(7) }, clt: TestContext.Current.CancellationToken);
        ExpireStorageService.IsOffline.Should().BeFalse();
    }

    [Fact]
    public async Task CancellationTokenTest()
    {
        const string cacheKey = "CancellationTokenTest";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var response = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse { Data = "test" }), clt: cts.Token);

        response.Should().BeNull();
    }

    [Fact]
    public async Task FailingTestToTestWorkflow()
    {
        Assert.True(false);
    }
}