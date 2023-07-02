namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// 
/// </summary>
public sealed record CreateAlbumRequestFromStream : AlbumCreationRequest
{
    /// <summary>
    /// 
    /// </summary>
    public required IEnumerable<StreamUploadRequest> Request { get; init; }
}