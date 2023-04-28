namespace CatBox.NET;

/// <summary>
/// Wraps a request to create a new album and add files to it
/// </summary>
public record CreateAlbumRequest
{
    /// <summary>
    /// UserHash code to associate the album with
    /// </summary>
    public string? UserHash { get; init; }

    /// <summary>
    /// The title of the album
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// An optional description for the album
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// A collection of already uploaded file URLs to put together in the album
    /// </summary>
    public required IEnumerable<string> Files { get; init; }
}
