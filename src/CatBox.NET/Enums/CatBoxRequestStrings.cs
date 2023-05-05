namespace CatBox.NET.Enums;

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
