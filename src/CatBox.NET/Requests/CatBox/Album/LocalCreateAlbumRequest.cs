namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Wraps a request to upload files to the API and create an album from those uploaded files
/// </summary>
public sealed record LocalCreateAlbumRequest : AlbumCreationRequest
{
    /// <summary>
    /// 
    /// </summary>
    public required IAsyncEnumerable<string?> Files { get; init; }
}