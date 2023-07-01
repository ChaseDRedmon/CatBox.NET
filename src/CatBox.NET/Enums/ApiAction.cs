using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CatBox.NET.Enums;

/// <summary>
/// A class for organizing actions that the library can take when talking to the API
/// </summary>
internal static class ApiAction
{
    /// <summary>
    /// UploadFile => "fileupload"
    /// </summary>
    public const string UploadFile = "fileupload";

    /// <summary>
    /// UrlUpload => "urlupload"
    /// </summary>
    public const string UrlUpload = "urlupload";

    /// <summary>
    /// DeleteFile => "deletefiles"
    /// </summary>
    public const string DeleteFile = "deletefiles";

    /// <summary>
    /// CreateAlbum => "createalbum"
    /// </summary>
    public const string CreateAlbum = "createalbum";

    /// <summary>
    /// EditAlbum => "editalbum"
    /// </summary>
    public const string EditAlbum = "editalbum";

    /// <summary>
    /// AddToAlbum => "addtoalbum"
    /// </summary>
    public const string AddToAlbum = "addtoalbum";

    /// <summary>
    /// RemoveFromAlbum => "removefromalbum"
    /// </summary>
    public const string RemoveFromAlbum = "removefromalbum";

    /// <summary>
    /// DeleteFromAlbum => "deletealbum"
    /// </summary>
    public const string DeleteFromAlbum = "deletealbum";
}