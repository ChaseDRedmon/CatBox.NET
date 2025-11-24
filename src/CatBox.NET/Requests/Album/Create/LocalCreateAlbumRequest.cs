namespace CatBox.NET.Requests.Album.Create;

/// <summary>
/// Wraps a request to upload files to the API and creates an album from those uploaded files
/// </summary>
public sealed record LocalCreateAlbumRequest : AlbumCreationRequestBase
{
    /// <summary>
    /// The files to upload to CatBox
    /// </summary>
    public required IAsyncEnumerable<string?> Files { get; init; }
}