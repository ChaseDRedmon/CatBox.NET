namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Wraps a request to add files, remove files, or delete an album
/// </summary>
public sealed record ModifyAlbumImagesRequest : Album
{
    /// <summary>
    /// The list of files associated with the album
    /// </summary>
    /// <remarks><see cref="Request"/> may alter the significance of this collection</remarks>
    public required IEnumerable<string> Files { get; init; }
}