using CatBox.NET.Enums;

namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// An abstract request representing parameters needed to work with the Album API
/// </summary>
public abstract record Album
{
    /// <summary>
    /// <see cref="RequestType"/>
    /// </summary>
    public required RequestType Request { get; init; }

    /// <summary>
    /// The User who owns this album
    /// </summary>
    public required string UserHash { get; init; }

    /// <summary>
    /// The unique identifier for the album
    /// </summary>
    public required string AlbumId { get; init; }
}