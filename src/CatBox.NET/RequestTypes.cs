namespace CatBox.NET;

/// <summary>
/// Currently unused
/// </summary>
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
/// Image expiry in litterbox.moe
/// </summary>
public enum ExpireAfter
{
    
    /// <summary>
    /// Expire after 1 hour
    /// </summary>
    OneHour,
    
    /// <summary>
    /// Expire after 12 hours
    /// </summary>
    TwelveHours,
    
    /// <summary>
    /// Expire after one day (24 hours)
    /// </summary>
    OneDay,
    
    /// <summary>
    /// Expire after three days (72 hours)
    /// </summary>
    ThreeDays
}

/// <summary>
/// Wraps multiple files to upload to the API
/// </summary>
public record FileUploadRequest
{
    public string? UserHash { get; init; }
    public required IEnumerable<FileInfo> Files { get; init; }
}

/// <summary>
/// 
/// </summary>
public record TemporaryFileUploadRequest
{
    public required ExpireAfter ExpireAfter { get; init; }
    public required IEnumerable<FileInfo> Files { get; init; }
}

/// <summary>
/// Wraps a network stream to stream content to the API
/// </summary>
public record StreamUploadRequest
{
    public string? UserHash { get; init; }
    public required string FileName { get; init; }
    public required Stream Stream { get; init; }
}

/// <summary>
/// 
/// </summary>
public record TemporaryStreamUploadRequest
{
    public required ExpireAfter ExpireAfter { get; init; }
    public required string FileName { get; init; }
    public required Stream Stream { get; init; }
}

/// <summary>
/// Wraps multiple URLs to upload to the API
/// </summary>
public record UrlUploadRequest
{
    public string? UserHash { get; init; }
    public required IEnumerable<Uri> Files { get; init; }
}

/// <summary>
/// Wraps a request to delete files from the API
/// </summary>
public record DeleteFileRequest
{
    public required string UserHash { get; init; }
    public required IEnumerable<string> FileNames { get; init; }
}

/// <summary>
/// Wraps a request to add files, remove files, or delete an album
/// </summary>
public record AlbumRequest
{
    public required CatBoxRequestTypes Request { get; init; }
    public required string UserHash { get; init; }
    public required string AlbumId { get; init; }
    public required IEnumerable<string> Files { get; init; }
}

/// <summary>
/// Wraps a request to edit an existing album with new files, new title, new description
/// </summary>
public record EditAlbumRequest
{
    public required string UserHash { get; init; }
    public required string AlbumId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IEnumerable<string> Files { get; init; }
}

/// <summary>
/// Wraps a request to create a new album and add files to it
/// </summary>
public record CreateAlbumRequest
{
    public string? UserHash { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required IEnumerable<string> Files { get; init; }
}

/// <summary>
/// A class for organizing request strings and arguments for the library and the API
/// </summary>
internal static class CatBoxRequestStrings
{
    /// <summary>
    /// Request API Argument in MultiPartForm
    /// </summary>
    public const string RequestType = "reqtype";
    
    /// <summary>
    /// UserHash API Argument in MultiPartForm
    /// </summary>
    public const string UserHashType = "userhash";
    
    /// <summary>
    /// Url API Argument in MultiPartForm
    /// </summary>
    public const string UrlType = "url";
    
    /// <summary>
    /// Files API Argument in MultiPartForm
    /// </summary>
    public const string FileType = "files";
    
    /// <summary>
    /// FileToUpload API Argument in MultiPartForm
    /// </summary>
    public const string FileToUploadType = "fileToUpload";
    
    /// <summary>
    /// Title API Argument in MultiPartForm
    /// </summary>
    public const string TitleType = "title";
    
    /// <summary>
    /// Description API Argument in MultiPartForm
    /// </summary>
    public const string DescriptionType = "desc";
    
    /// <summary>
    /// Album Id API Argument in MultiPartForm
    /// </summary>
    public const string AlbumIdShortType = "short";

    public const string ExpiryType = "time";
    
    private static string UploadFile => "fileupload";
    private static string UrlUpload => "urlupload";
    private static string DeleteFile => "deletefiles";
    private static string CreateAlbum => "createalbum";
    private static string EditAlbum => "editalbum";
    private static string AddToAlbum => "addtoalbum";
    private static string RemoveFromAlbum => "removefromalbum";
    private static string DeleteFromAlbum => "deletealbum";
    
    /// <summary>
    /// Converts a <see cref="CatBoxRequestTypes"/> to the CatBox.moe equivalent API parameter string
    /// </summary>
    /// <param name="requestTypes">A request type</param>
    /// <returns>CatBox API Request String</returns>
    /// <exception cref="ArgumentOutOfRangeException"> when an invalid request type is chosen</exception>
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
    
    /// <summary>
    /// Converts a <see cref="ExpireAfter"/> value to the Litterbox.moe API equivalent time string
    /// </summary>
    /// <param name="expiry">Amount of time before an image expires and is deleted</param>
    /// <returns>Litterbox API Time Equivalent parameter value</returns>
    /// <exception cref="ArgumentOutOfRangeException"> when an invalid expiry value is chosen</exception>
    public static string ToRequest(this ExpireAfter expiry) =>
        expiry switch
        {
            ExpireAfter.OneHour => "1h",
            ExpireAfter.TwelveHours => "12h",
            ExpireAfter.OneDay => "24h",
            ExpireAfter.ThreeDays => "72h",
            _ => throw new ArgumentOutOfRangeException(nameof(expiry), expiry, null)
        };
}

internal static class Common
{
    /// <summary>
    /// These file extensions are not allowed by the API, so filter them out
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static bool IsFileExtensionValid(string extension)
    {
        switch (extension)
        {
            case ".exe":
            case ".scr":
            case ".cpl":
            case var _ when extension.Contains(".doc"):
            case ".jar":
                return false;
            default:
                return true;
        }
    }
}
