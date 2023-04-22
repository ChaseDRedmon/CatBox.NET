namespace CatBox.NET;

public enum UploadHost
{
    CatBox,
    LitterBox
}

public enum CatBoxRequestTypes
{
    UploadFile,
    UrlUpload,
    DeleteFile,
    CreateAlbum,
    EditAlbum,
    AddToAlbum,
    RemoveFromAlbum,
    DeleteAlbum
}

public record CreateAlbumRequest
{
    public string? UserHash { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}

public record FileUploadRequest
{
    public string? UserHash { get; init; }
    public required IEnumerable<FileInfo> Files { get; init; }
}

public record UrlUploadRequest
{
    public string? UserHash { get; init; }
    public required IEnumerable<Uri?> Files { get; init; }
}

public record DeleteFileRequest
{
    public required string UserHash { get; init; }
    public required IEnumerable<string?> FileNames { get; init; }
}

public record AlbumRequest
{
    public CatBoxRequestTypes Request { get; init; }
    public string? UserHash { get; init; }
    public string? AlbumId { get; init; }
    public required IEnumerable<FileInfo?> Files { get; init; }
}

internal static class CatBoxRequestStrings
{
    public const string RequestType = "reqtype";
    public const string UserHashType = "userhash";
    public const string UrlType = "url";
    public const string FileType = "files";
    
    private static string UploadFile => "fileupload";
    private static string UrlUpload => "urlupload";
    private static string DeleteFile => "deletefiles";
    private static string CreateAlbum => "createalbum";
    private static string EditAlbum => "editalbum";
    private static string AddToAlbum => "addtoalbum";
    private static string RemoveFromAlbum => "removefromalbum";
    private static string DeleteFromAlbum => "deletealbum";
    
    public static string ToRequest(this CatBoxRequestTypes requestTypes) =>
        requestTypes switch
        {
            CatBoxRequestTypes.UploadFile => UploadFile,
            CatBoxRequestTypes.UrlUpload => UrlUpload,
            CatBoxRequestTypes.DeleteFile => DeleteFile,
            CatBoxRequestTypes.CreateAlbum => CreateAlbum,
            CatBoxRequestTypes.EditAlbum => EditAlbum,
            CatBoxRequestTypes.AddToAlbum => AddToAlbum,
            CatBoxRequestTypes.RemoveFromAlbum => RemoveFromAlbum,
            CatBoxRequestTypes.DeleteAlbum => DeleteFromAlbum,
            _ => throw new ArgumentOutOfRangeException(nameof(requestTypes), requestTypes, null)
        };
}