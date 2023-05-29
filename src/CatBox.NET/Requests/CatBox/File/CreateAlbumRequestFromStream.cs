namespace CatBox.NET.Requests.CatBox;

public sealed record CreateAlbumRequestFromStream : AlbumCreationRequest
{
    public required IEnumerable<StreamUploadRequest> Request { get; init; }
}