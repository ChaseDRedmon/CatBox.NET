namespace CatBox.NET.Requests.CatBox;

public record CreateAlbumRequestFromFiles : AlbumCreationRequest
{
    public required FileUploadRequest UploadRequest { get; init; }
}