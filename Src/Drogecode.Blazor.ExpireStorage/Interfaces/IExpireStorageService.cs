using System.Diagnostics.CodeAnalysis;

namespace Drogecode.Blazor.ExpireStorage;

public interface IExpireStorageService
{
    Task<TRes?> CachedRequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRes>(string cacheKey, Func<Task<TRes>> function, CachedRequest? request = null, CancellationToken clt = default);

}