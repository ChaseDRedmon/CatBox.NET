namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// 
/// </summary>
public sealed record CreateAlbumRequestFromFiles : AlbumCreationRequest
{
    /// <summary>
    /// 
    /// </summary>
    public required FileUploadRequest UploadRequest { get; init; }
}