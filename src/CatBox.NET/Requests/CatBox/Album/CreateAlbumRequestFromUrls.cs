namespace CatBox.NET.Requests.CatBox;

public sealed record CreateAlbumRequestFromUrls : AlbumCreationRequest
{
    public UrlUploadRequest UrlUploadRequest { get; init; }
}