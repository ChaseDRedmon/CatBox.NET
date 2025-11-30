namespace CatBox.NET.Responses.Album;

/// <summary>
/// Processed album information with parsed file list
/// </summary>
public sealed record AlbumInfo
{
    /// <summary>Album title</summary>
    public required string Title { get; init; }

    /// <summary>Album description</summary>
    public required string Description { get; init; }

    /// <summary>Album ID (short code)</summary>
    public required string AlbumId { get; init; }

    /// <summary>Date the album was created</summary>
    public required DateOnly DateCreated { get; init; }

    /// <summary>List of file names in the album</summary>
    public required string[] Files { get; init; }
}
