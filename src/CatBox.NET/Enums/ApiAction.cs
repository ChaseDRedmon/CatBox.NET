using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CatBox.NET.Enums;

/// <summary>
/// A class for 
/// </summary>
internal static class ApiAction
{
    /// <summary>
    /// UploadFile => "fileupload"
    /// </summary>
    public static string UploadFile => "fileupload";
    
    /// <summary>
    /// UrlUpload => "urlupload"
    /// </summary>
    public static string UrlUpload => "urlupload";
    
    /// <summary>
    /// DeleteFile => "deletefiles"
    /// </summary>
    public static string DeleteFile => "deletefiles";
    
    /// <summary>
    /// CreateAlbum => "createalbum"
    /// </summary>
    public static string CreateAlbum => "createalbum";
    
    /// <summary>
    /// EditAlbum => "editalbum"
    /// </summary>
    public static string EditAlbum => "editalbum";
    
    /// <summary>
    /// AddToAlbum => "addtoalbum"
    /// </summary>
    public static string AddToAlbum => "addtoalbum";
    
    /// <summary>
    /// RemoveFromAlbum => "removefromalbum"
    /// </summary>
    public static string RemoveFromAlbum => "removefromalbum";
    
    /// <summary>
    /// DeleteFromAlbum => "deletealbum"
    /// </summary>
    public static string DeleteFromAlbum => "deletealbum";
}