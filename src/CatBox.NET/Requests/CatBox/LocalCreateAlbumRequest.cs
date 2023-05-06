namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Wraps a request to upload files to the API and create an album from those uploaded files
/// </summary>
public record LocalCreateAlbumRequest : AlbumCreationRequest
{
    /// <summary>
    /// A collection of already uploaded file URLs to put together in the album
    /// </summary>
    public required IAsyncEnumerable<string?> Files { get; init; }
}