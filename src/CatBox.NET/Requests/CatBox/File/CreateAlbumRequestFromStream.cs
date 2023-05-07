namespace CatBox.NET.Requests.CatBox;

public record CreateAlbumRequestFromStream : AlbumCreationRequest
{
    public required StreamUploadRequest Request { get; init; }
}