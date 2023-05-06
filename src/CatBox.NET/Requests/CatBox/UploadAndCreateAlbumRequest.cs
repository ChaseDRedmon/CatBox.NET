namespace CatBox.NET.Requests.CatBox;

public record UploadAndCreateAlbumRequest : AlbumCreationRequest
{
    public required FileUploadRequest UploadRequest { get; init; }
}