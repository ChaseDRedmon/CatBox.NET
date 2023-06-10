namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// 
/// </summary>
public sealed record CreateAlbumRequestFromUrls : AlbumCreationRequest
{
    /// <summary>
    /// 
    /// </summary>
    public UrlUploadRequest UrlUploadRequest { get; init; }
}