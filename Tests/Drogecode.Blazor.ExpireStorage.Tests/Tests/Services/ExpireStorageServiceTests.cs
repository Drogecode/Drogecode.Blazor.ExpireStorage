using Drogecode.Blazor.ExpireStorage.Tests.Models;

namespace Drogecode.Blazor.ExpireStorage.Tests.Tests.Services;

public class ExpireStorageServiceTests
{
    private readonly IExpireStorageService _expireStorageService;

    public ExpireStorageServiceTests(IExpireStorageService expireStorageService)
    {
        _expireStorageService = expireStorageService;
    }

    [Fact]
    public async Task MinimalRequestTest()
    {
        const string cacheKey = "MinimalRequestTest";
        var response = await _expireStorageService.CachedRequestAsync<string>(cacheKey, () => Task.FromResult("test"), clt: TestContext.Current.CancellationToken);
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
        }), clt: TestContext.Current.CancellationToken);
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
            clt: TestContext.Current.CancellationToken);
        Assert.NotNull(addToCache?.Data);
        addToCache.HandledBy.Should().Be(HandledBy.Function);
        var response = await _expireStorageService.CachedRequestAsync<TestStringResponse>(cacheKey, () => Task.FromResult(new TestStringResponse
            {
                Data = "not called"
            }),
            new CachedRequest { OneCallPerCache = true },
            clt: TestContext.Current.CancellationToken);
        Assert.NotNull(response?.Data);
        response.HandledBy.Should().Be(HandledBy.Cache);
        response.Data.Should().Be("test");
    }
}