using System.Diagnostics.CodeAnalysis;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;

namespace CatBox.NET.Client;

internal static class Common
{
    /// <summary>
    /// These file extensions are not allowed by the API, so filter them out
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <returns><see langword="true"/> if the file extension is valid; otherwise, <see langword="false"/></returns>
    public static bool IsFileExtensionValid(FileInfo file)
    {
        var extension = file.Extension;
        return extension switch
        {
            ".exe" or ".scr" or ".cpl" or ".jar" => false,
            _ when extension.Contains(".doc") => false,
            _ => true
        };
    }

    /// <summary>
    /// Validates an Album Creation Request
    /// </summary>
    /// <param name="requestBase">The album creation requestBase to validate</param>
    /// <exception cref="ArgumentNullException">when the requestBase is null</exception>
    /// <exception cref="ArgumentNullException">when the description is null</exception>
    /// <exception cref="ArgumentNullException">when the title is null</exception>
    public static void ThrowIfAlbumCreationRequestIsInvalid(AlbumCreationRequestBase requestBase)
    {
        ArgumentNullException.ThrowIfNull(requestBase);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestBase.Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestBase.Title);
    }
    
    /// <summary>
    /// 1. Filter Invalid Request Types on the Album Endpoint <br/>
    /// 2. Check that the user hash is not null, empty, or whitespace when attempting to modify or delete an album. User hash is required for those operations
    /// </summary>
    /// <param name="imagesRequest"></param>
    /// <returns></returns>
    public static bool IsAlbumRequestTypeValid(ModifyAlbumImagesRequest imagesRequest)
    {
        var request = imagesRequest.Request;
        var hasUserHash = !string.IsNullOrWhiteSpace(imagesRequest.UserHash);

        if (request == RequestType.CreateAlbum)
            return true;

        return (request == RequestType.EditAlbum ||
                request == RequestType.AddToAlbum ||
                request == RequestType.RemoveFromAlbum ||
                request == RequestType.DeleteAlbum) && hasUserHash;
    }
}