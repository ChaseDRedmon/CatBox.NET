namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Creates an album on CatBox from files that are uploaded from the PC
/// </summary>
public sealed record CreateAlbumRequestFromFiles : AlbumCreationRequest
{
    /// <summary>
    /// 
    /// </summary>
    public required FileUploadRequest UploadRequest { get; init; }
}