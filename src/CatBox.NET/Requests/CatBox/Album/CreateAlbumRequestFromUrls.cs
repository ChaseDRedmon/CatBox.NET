namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Creates an album on CatBox from a collection of URLs that are uploaded 
/// </summary>
public sealed record CreateAlbumRequestFromUrls : AlbumCreationRequest
{
    /// <summary>
    /// 
    /// </summary>
    public UrlUploadRequest UrlUploadRequest { get; init; }
}