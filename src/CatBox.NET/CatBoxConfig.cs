namespace CatBox.NET;

/// <summary>
/// Configuration object for storing URLs to the API
/// </summary>
public record CatBoxConfig
{
    /// <summary>
    /// URL for the catbox.moe domain
    /// </summary>
    public Uri? CatBoxUrl { get; set; }
    
    /// <summary>
    /// URL for the litterbox.moe domain
    /// </summary>
    public Uri? LitterboxUrl { get; set; }
}