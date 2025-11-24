using Intellenum;

namespace CatBox.NET.Enums;

/// <summary>
/// Types used for CatBox
/// </summary>
[Intellenum(typeof(string))]
[Member("UploadFile", "fileupload")]
[Member("UrlUpload", "urlupload")]
[Member("DeleteFile", "deletefiles")]
[Member("CreateAlbum", "createalbum")]
[Member("EditAlbum", "editalbum")]
[Member("AddToAlbum", "addtoalbum")]
[Member("RemoveFromAlbum", "removefromalbum")]
[Member("DeleteAlbum", "deletealbum")]
public sealed partial class RequestType;
