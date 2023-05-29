namespace CatBox.NET.Requests.CatBox;

public sealed record CreateAlbumRequestFromFiles : AlbumCreationRequest
{
    public required FileUploadRequest UploadRequest { get; init; }
}