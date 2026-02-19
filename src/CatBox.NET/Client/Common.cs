using System.Runtime.CompilerServices;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album.Modify;

namespace CatBox.NET.Client;

internal static class Common
{
    /// <summary>
    /// Maximum number of files allowed in a CatBox album
    /// </summary>
    public const int MaxAlbumFiles = 500;

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

    /// <param name="url">The URL or file name</param>
    extension(string? url)
    {
        /// <summary>
        /// Extracts the file name from a CatBox file URL (files.catbox.moe) or returns the original string.
        /// </summary>
        /// <example>"https://files.catbox.moe/abc123.jpg" → "abc123.jpg"</example>
        /// <returns>The extracted file name or original string</returns>
        public string? ToCatboxImageName()
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (url.Contains("files.catbox.moe"))
                return url.Split('/')[^1];

            return url;
        }

        /// <summary>
        /// Extracts the album short code from a CatBox album URL (catbox.moe/c/) or returns the original string.
        /// </summary>
        /// <example>"https://catbox.moe/c/abc123" → "abc123"</example>
        /// <returns>The extracted album short code or original string</returns>
        public string? ToAlbumShortCode()
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (url.Contains("catbox.moe/c/"))
                return url.Split('/')[^1];

            return url;
        }
    }

    /// <param name="source">The async enumerable source</param>
    extension<T>(IAsyncEnumerable<T> source)
    {
        /// <summary>
        /// Asynchronously collects all elements from an IAsyncEnumerable into a List
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A list containing all elements</returns>
        public async Task<List<T>> ToListAsync(CancellationToken ct = default)
        {
            var list = new List<T>();
            await foreach (var item in source.WithCancellation(ct).ConfigureAwait(false))
            {
                list.Add(item);
            }
            return list;
        }
    }
}