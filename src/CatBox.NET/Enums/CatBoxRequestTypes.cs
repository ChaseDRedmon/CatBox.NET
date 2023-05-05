namespace CatBox.NET.Enums;

/// <summary>
/// Types used for CatBox
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
