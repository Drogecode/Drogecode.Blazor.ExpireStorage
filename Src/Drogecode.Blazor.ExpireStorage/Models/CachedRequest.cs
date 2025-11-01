namespace Drogecode.Blazor.ExpireStorage.Models;

public class CachedRequest
{
    /// <summary>
    /// One call per session, but not expired.
    /// </summary>
    public bool OneCallPerSession { get; set; }

    /// <summary>
    /// One call per local storage cache, but not expired.
    /// </summary>
    public bool OneCallPerCache { get; set; }
    
    /// <summary>
    /// Will be deleted from local storge when this time has passed. UTC
    /// </summary>
    public DateTime ExpireLocalStorage { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>
    /// If one call per session, expire after set time UTC
    /// </summary>
    public DateTime ExpireSession { get; set; } = DateTime.UtcNow.AddMinutes(15);

    /// <summary>
    /// Ignore session cache
    /// </summary>
    public bool ForceCache { get; set; }

    /// <summary>
    /// Return cached but also call for update
    /// </summary>
    public bool CachedAndReplace { get; set; }

    /// <summary>
    /// Retry once on JsonException.
    /// </summary>
    public bool RetryOnJsonException { get; set; } = true;
}