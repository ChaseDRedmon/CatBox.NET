namespace CatBox.NET.Enums;

/// <summary>
/// API Request parameters for request content and request types
/// </summary>
internal static class RequestParameters
{
    /// <summary>
    /// Request API Argument in MultiPartForm
    /// </summary>
    public const string Request = "reqtype";
    
    /// <summary>
    /// UserHash API Argument in MultiPartForm
    /// </summary>
    public const string UserHash = "userhash";
    
    /// <summary>
    /// Url API Argument in MultiPartForm
    /// </summary>
    public const string Url = "url";
    
    /// <summary>
    /// Files API Argument in MultiPartForm
    /// </summary>
    public const string Files = "files";
    
    /// <summary>
    /// FileToUpload API Argument in MultiPartForm
    /// </summary>
    public const string FileToUpload = "fileToUpload";
    
    /// <summary>
    /// Title API Argument in MultiPartForm
    /// </summary>
    public const string Title = "title";
    
    /// <summary>
    /// Description API Argument in MultiPartForm
    /// </summary>
    public const string Description = "desc";
    
    /// <summary>
    /// Album Id API Argument in MultiPartForm
    /// </summary>
    public const string AlbumIdShort = "short";
    
    /// <summary>
    /// Expiry time API Argument for Litterbox MultiPartForm
    /// </summary>
    public const string Expiry = "time";
}