namespace CatBox.NET.Responses.Album;

/// <summary>
/// Result of uploading files to an album with capacity awareness
/// </summary>
public sealed record AlbumUploadResult
{
    /// <summary>The album URL after successful modification, or null if no files were uploaded</summary>
    public required string? AlbumUrl { get; init; }

    /// <summary>Number of files successfully uploaded and added to the album</summary>
    public required int FilesUploaded { get; init; }

    /// <summary>Number of files that were skipped due to album capacity limit</summary>
    public required int FilesSkipped { get; init; }

    /// <summary>Remaining capacity in the album after this operation</summary>
    public required int RemainingCapacity { get; init; }

    /// <summary>Whether the album has reached its maximum capacity of 500 files</summary>
    public bool ReachedLimit => RemainingCapacity == 0;
}
