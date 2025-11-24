using AnyOfTypes;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;

namespace CatBox.NET.Requests.Album.Create;

/// <summary>
/// 
/// </summary>
public sealed record CreateAlbumRequest : AlbumCreationRequestBase, IAlbumUploadRequest
{
    public required AnyOf<FileUploadRequest, IEnumerable<StreamUploadRequest>, UrlUploadRequest> UploadRequest { get; init; }
}