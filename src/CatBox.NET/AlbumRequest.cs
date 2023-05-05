namespace CatBox.NET;

/// <summary>
/// Wraps a request to add files, remove files, or delete an album
/// </summary>
public record AlbumRequest
{
    /// <summary>
    /// <see cref="CatBoxRequestTypes"/>
    /// </summary>
    public required CatBoxRequestTypes Request { get; init; }

    /// <summary>
    /// The User who owns this album
    /// </summary>
    public required string UserHash { get; init; }

    /// <summary>
    /// The unique identifier for the album
    /// </summary>
    public required string AlbumId { get; init; }

    /// <summary>
    /// The list of files associated with the album
    /// </summary>
    /// <remarks><see cref="Request"/> may alter the significance of this collection</remarks>
    public required IEnumerable<string> Files { get; init; }
}
