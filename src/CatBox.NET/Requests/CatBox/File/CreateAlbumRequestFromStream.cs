namespace CatBox.NET.Requests.CatBox;

public sealed record CreateAlbumRequestFromStream : AlbumCreationRequest
{
    public required StreamUploadRequest Request { get; init; }
}