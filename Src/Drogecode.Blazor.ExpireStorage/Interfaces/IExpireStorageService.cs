using System.Diagnostics.CodeAnalysis;

namespace Drogecode.Blazor.ExpireStorage;

public interface IExpireStorageService
{
    /// <summary>
    /// Caches the request and returns the response.
    /// </summary>
    /// <param name="cacheKey">Uniek key for storage</param>
    /// <param name="function">Function to run</param>
    /// <param name="request">Object with configuration</param>
    /// <param name="defaultResponse">Response if function fails</param>
    /// <param name="clt">Cancelation token</param>
    /// <typeparam name="TRes">Type of return object</typeparam>
    /// <returns>Response from either function, cache, or default response</returns>
    Task<TRes?> CachedRequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRes>(string cacheKey, Func<Task<TRes>> function,
        CachedRequest? request = null, TRes? defaultResponse = default, CancellationToken clt = default);
}