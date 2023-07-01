using CatBox.NET.Enums;

namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// 
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