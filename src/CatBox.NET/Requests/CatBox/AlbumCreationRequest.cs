namespace CatBox.NET.Requests.CatBox;

public abstract record AlbumCreationRequest
{
    /// <summary>
    /// The title of the album
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// An optional description for the album
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// UserHash code to associate the album with
    /// </summary>
    public string? UserHash { get; init; }
}