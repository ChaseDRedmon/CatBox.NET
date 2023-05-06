namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Wraps a request to create a new album and existing API files to it
/// </summary>
public record RemoteCreateAlbumRequest : AlbumCreationRequest
{
    /// <summary>
    /// A collection of already uploaded file URLs to put together in the album
    /// </summary>
    public required IEnumerable<string?> Files { get; init; }
}
