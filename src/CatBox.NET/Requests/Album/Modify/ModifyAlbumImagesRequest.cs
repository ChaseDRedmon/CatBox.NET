namespace CatBox.NET.Requests.Album.Modify;

/// <summary>
/// Wraps a requestBase to add files, remove files, or delete an album
/// </summary>
public sealed record ModifyAlbumImagesRequest : AlbumBase
{
    /// <summary>
    /// The list of files associated with the album
    /// </summary>
    /// <remarks><see cref="Request"/> may alter the significance of this collection</remarks>
    public required IEnumerable<string> Files { get; init; }
}