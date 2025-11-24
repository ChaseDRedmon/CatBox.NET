using CatBox.NET.Enums;
using CatBox.NET.Requests.Album.Modify;

namespace CatBox.NET.Client;

/// <summary>
/// Provides helper methods for throwing exceptions with consistent error handling
/// </summary>
internal static class Throw
{
    /// <summary>
    /// Throws <see cref="Exceptions.CatBoxFileSizeLimitExceededException"/> if the file size exceeds the maximum allowed size for CatBox
    /// </summary>
    /// <param name="fileSize">The size of the file in bytes</param>
    /// <param name="maxSize">The maximum allowed file size in bytes</param>
    /// <exception cref="Exceptions.CatBoxFileSizeLimitExceededException">When file size exceeds the maximum</exception>
    public static void IfCatBoxFileSizeExceeds(long fileSize, long maxSize)
    {
        if (fileSize > maxSize)
            throw new Exceptions.CatBoxFileSizeLimitExceededException(fileSize);
    }

    /// <summary>
    /// Throws <see cref="Exceptions.LitterboxFileSizeLimitExceededException"/> if the file size exceeds the maximum allowed size for Litterbox
    /// </summary>
    /// <param name="fileSize">The size of the file in bytes</param>
    /// <param name="maxSize">The maximum allowed file size in bytes</param>
    /// <exception cref="Exceptions.LitterboxFileSizeLimitExceededException">When file size exceeds the maximum</exception>
    public static void IfLitterboxFileSizeExceeds(long fileSize, long maxSize)
    {
        if (fileSize > maxSize)
            throw new Exceptions.LitterboxFileSizeLimitExceededException(fileSize);
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> if the album request type is invalid
    /// </summary>
    /// <param name="isValid">Whether the request type is valid</param>
    /// <param name="paramName">The name of the parameter that is invalid</param>
    /// <exception cref="ArgumentException">When the request type is invalid for the album endpoint</exception>
    public static void IfAlbumRequestTypeInvalid(bool isValid, string paramName)
    {
        if (!isValid)
            throw new ArgumentException("Invalid Request Type for album endpoint", paramName);
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if the album request type is not supported for the operation
    /// </summary>
    /// <param name="request">The request type to validate</param>
    /// <param name="allowedTypes">The allowed request types for this operation</param>
    /// <exception cref="InvalidOperationException">When the request type is not in the allowed types</exception>
    public static void IfAlbumOperationInvalid(RequestType request, params RequestType[] allowedTypes)
    {
        if (allowedTypes.Any(allowedType => request == allowedType))
        {
            return;
        }

        throw new InvalidOperationException("Invalid Request Type for album endpoint");
    }
}
