using AnyOfTypes;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;

namespace CatBox.NET.Requests.Album.Modify;

/// <summary>
/// A request for uploading files into an existing CatBox Album
/// </summary>
public sealed record UploadToAlbumRequest : AlbumBase, IAlbumUploadRequest
{
    /// <summary>
    /// The CatBox Upload request to upload files into an existing album
    /// </summary>
    public required AnyOf<FileUploadRequest, IEnumerable<StreamUploadRequest>, UrlUploadRequest> UploadRequest { get; init; }
}