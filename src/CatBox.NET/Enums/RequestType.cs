using BetterEnumsGen;
using CatBox.NET.Client.Attributes;

namespace CatBox.NET.Enums;

/// <summary>
/// Types used for CatBox
/// </summary>
[BetterEnum]
public enum RequestType
{
    /// <summary>
    /// UploadFile => "fileupload"
    /// </summary>
    [ApiValue("fileupload")]
    UploadFile,
    
    /// <summary>
    /// UrlUpload => "urlupload"
    /// </summary>
    [ApiValue("urlupload")]
    UrlUpload,
    
    /// <summary>
    /// DeleteFile => "deletefiles"
    /// </summary>
    [ApiValue("deletefiles")]
    DeleteFile,
    
    /// <summary>
    /// CreateAlbum => "createalbum"
    /// </summary>
    [ApiValue("createalbum")]
    CreateAlbum,
    
    /// <summary>
    /// EditAlbum => "editalbum"
    /// </summary>
    [ApiValue("editalbum")]
    EditAlbum,
    
    /// <summary>
    /// AddToAlbum => "addtoalbum"
    /// </summary>
    [ApiValue("addtoalbum")]
    AddToAlbum,
    
    /// <summary>
    /// RemoveFromAlbum => "removefromalbum"
    /// </summary>
    [ApiValue("removefromalbum")]
    RemoveFromAlbum,
    
    /// <summary>
    /// DeleteFromAlbum => "deletealbum"
    /// </summary>
    [ApiValue("deletealbum")]
    DeleteAlbum
}
