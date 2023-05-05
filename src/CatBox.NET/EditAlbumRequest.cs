namespace CatBox.NET;

/// <summary>
/// Wraps a request to edit an existing album with new files, new title, new description
/// </summary>
public record EditAlbumRequest
{
    /// <summary>
    /// The UserHash that owns the album
    /// </summary>
    public required string UserHash { get; init; }

    /// <summary>
    /// The unique ID of the album
    /// </summary>
    public required string AlbumId { get; init; }

    /// <summary>
    /// The new title of the album
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The new description of the album
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// The collection of files to associate together for the album
    /// </summary>
    public required IEnumerable<string> Files { get; init; }
}
