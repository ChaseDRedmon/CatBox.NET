namespace CatBox.NET.Requests.Album;

/// <summary>
/// Request to retrieve the list of files in an album
/// </summary>
public sealed record GetAlbumRequest
{
    /// <summary>
    /// The unique identifier for the album (API value: "short")
    /// </summary>
    public required string AlbumId { get; init; }
}
