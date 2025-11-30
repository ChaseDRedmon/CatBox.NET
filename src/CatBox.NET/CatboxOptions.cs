namespace CatBox.NET;

/// <summary>
/// Configuration object for storing URLs to the API
/// </summary>
public sealed record CatboxOptions
{
    /// <summary>
    /// URL for the catbox.moe domain
    /// </summary>
    public Uri? CatBoxUrl { get; set; }

    /// <summary>
    /// Base URL for downloading CatBox files (default: https://files.catbox.moe/)
    /// </summary>
    public Uri CatBoxFilesUrl { get; set; } = new("https://files.catbox.moe/");

    /// <summary>
    /// URL for the litterbox.moe domain
    /// </summary>
    public Uri? LitterboxUrl { get; set; }
}