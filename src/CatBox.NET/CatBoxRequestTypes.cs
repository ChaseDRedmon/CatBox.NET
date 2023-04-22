namespace CatBox.NET;

public enum UploadHost
{
    CatBox,
    LitterBox
}

/// <summary>
/// 
/// </summary>
public enum CatBoxRequestTypes
{
    /// <summary>
    /// UploadFile => "fileupload"
    /// </summary>
    UploadFile,
    
    /// <summary>
    /// UrlUpload => "urlupload"
    /// </summary>
    UrlUpload,
    
    /// <summary>
    /// DeleteFile => "deletefiles"
    /// </summary>
    DeleteFile,
    
    /// <summary>
    /// CreateAlbum => "createalbum"
    /// </summary>
    CreateAlbum,
    
    /// <summary>
    /// EditAlbum => "editalbum"
    /// </summary>
    EditAlbum,
    
    /// <summary>
    /// AddToAlbum => "addtoalbum"
    /// </summary>
    AddToAlbum,
    
    /// <summary>
    /// RemoveFromAlbum => "removefromalbum"
    /// </summary>
    RemoveFromAlbum,
    
    /// <summary>
    /// DeleteFromAlbum => "deletealbum"
    /// </summary>
    DeleteAlbum
}

/// <summary>
/// 
/// </summary>
public record FileUploadRequest
{
    public string? UserHash { get; init; }
    public required IEnumerable<FileInfo> Files { get; init; }
}

public record StreamUploadRequest
{
    public string? UserHash { get; init; }
    
    public required string FileName { get; init; }
    public required Stream Stream { get; init; }
}

/// <summary>
/// 
/// </summary>
public record UrlUploadRequest
{
    public string? UserHash { get; init; }
    public required IEnumerable<Uri?> Files { get; init; }
}

/// <summary>
/// 
/// </summary>
public record DeleteFileRequest
{
    public required string UserHash { get; init; }
    public required IEnumerable<string?> FileNames { get; init; }
}

/// <summary>
/// 
/// </summary>
public record AlbumRequest
{
    public CatBoxRequestTypes Request { get; init; }
    public string? UserHash { get; init; }
    public string? AlbumId { get; init; }
    public required IEnumerable<string?> Files { get; init; }
}

public record CreateAlbumRequest
{
    public string? UserHash { get; init; }
    public required string? Title { get; init; }
    public required string? Description { get; init; }
    public required IEnumerable<string?> Files { get; init; }
}

internal static class CatBoxRequestStrings
{
    public const string RequestType = "reqtype";
    public const string UserHashType = "userhash";
    public const string UrlType = "url";
    public const string FileType = "files";
    public const string FileToUploadType = "fileToUpload";
    public const string TitleType = "title";
    public const string DescriptionType = "desc";
    public const string AlbumIdShortType = "short";
    
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