namespace CatBox.NET.Requests.Album.Create;

/// <summary>
/// Wraps a request to create a new album with files that have been uploaded to the API already
/// </summary>
public sealed record RemoteCreateAlbumRequest : AlbumCreationRequestBase
{
    /// <summary>
    /// A collection of already uploaded file URLs to put together in the album
    /// </summary>
    public required IEnumerable<string?> Files { get; init; }
}