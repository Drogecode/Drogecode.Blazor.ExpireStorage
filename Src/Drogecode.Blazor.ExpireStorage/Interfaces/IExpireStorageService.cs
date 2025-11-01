using System.Diagnostics.CodeAnalysis;
using Drogecode.Blazor.ExpireStorage.Models;

namespace Drogecode.Blazor.ExpireStorage.Interfaces;

public interface IExpireStorageService
{
    Task<TRes?> CachedRequestAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRes>(string cacheKey, Func<Task<TRes>> function, CachedRequest? request = null, CancellationToken clt = default);

}