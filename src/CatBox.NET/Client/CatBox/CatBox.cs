using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CatBox.NET.Requests.Album;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;
using CatBox.NET.Responses.Album;

namespace CatBox.NET.Client;

/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
public interface ICatBox
{
    /// <summary>
    /// Creates an album on CatBox from files that are uploaded in the requestBase
    /// </summary>
    /// <param name="requestFromFiles">Album Creation Request</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns></returns>
    Task<string?> CreateAlbumFromFilesAsync(CreateAlbumRequest requestFromFiles, CancellationToken ct = default);

    /// <summary>
    /// Upload and add images to an existing Catbox Album
    /// </summary>
    /// <param name="request">Album Creation Request</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns></returns>
    Task<string?> UploadImagesToAlbumAsync(UploadToAlbumRequest request, CancellationToken ct = default);

    /// <summary>
    /// Uploads files to an existing album, respecting the 500-file limit.
    /// Fetches album first to determine available capacity and uploads only what fits.
    /// </summary>
    /// <param name="request">Album upload request</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Result containing upload counts and remaining capacity</returns>
    Task<AlbumUploadResult> UploadImagesToAlbumSafeAsync(UploadToAlbumRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets album information including the list of files
    /// </summary>
    /// <param name="albumId">The album ID to retrieve</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Album information with parsed file list</returns>
    Task<AlbumInfo> GetAlbumAsync(string albumId, CancellationToken ct = default);

    /// <summary>
    /// Downloads a file from CatBox to the specified directory
    /// </summary>
    /// <param name="fileName">The file name (e.g., "abc123.png")</param>
    /// <param name="destination">Directory to save the file</param>
    /// <param name="ct">Cancellation Token</param>
    /// <remarks>Skips download if file already exists at destination</remarks>
    Task DownloadFileAsync(string fileName, DirectoryInfo destination, CancellationToken ct = default);

    /// <summary>
    /// Downloads a file from CatBox to the specified path
    /// </summary>
    /// <param name="fileName">The file name (e.g., "abc123.png")</param>
    /// <param name="destinationPath">Directory path to save the file</param>
    /// <param name="ct">Cancellation Token</param>
    /// <remarks>Skips download if file already exists at destination</remarks>
    Task DownloadFileAsync(string fileName, [StringSyntax(StringSyntaxAttribute.Uri)] string destinationPath, CancellationToken ct = default);

    /// <summary>
    /// Downloads all files from an album to the specified directory
    /// </summary>
    /// <param name="albumId">The album ID to download</param>
    /// <param name="destination">Directory to save the files</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Yields FileInfo for each downloaded file</returns>
    IAsyncEnumerable<FileInfo> DownloadAlbumAsync(string albumId, DirectoryInfo destination, CancellationToken ct = default);

    /// <summary>
    /// Downloads all files from an album to the specified path
    /// </summary>
    /// <param name="albumId">The album ID to download</param>
    /// <param name="destinationPath">Directory path to save the files</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Yields FileInfo for each downloaded file</returns>
    IAsyncEnumerable<FileInfo> DownloadAlbumAsync(string albumId, [StringSyntax(StringSyntaxAttribute.Uri)] string destinationPath, CancellationToken ct = default);
}

/// <inheritdoc/>
/// <summary>
/// Instantiate a new catbox class 
/// </summary>
/// <param name="client">The CatBox Api Client (<see cref="ICatBoxClient"/>)</param>
public sealed class Catbox(ICatBoxClient client) : ICatBox
{
    /// <inheritdoc/>
    public async Task<string?> CreateAlbumFromFilesAsync(CreateAlbumRequest requestFromFiles, CancellationToken ct = default)
    {
        var uploadedFiles = await Upload(requestFromFiles, ct).ToListAsync(ct).ConfigureAwait(false);

        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromFiles.Title,
            Description = requestFromFiles.Description,
            UserHash = requestFromFiles.UserHash,
            Files = uploadedFiles
        };

        return await client.CreateAlbumAsync(createAlbumRequest, ct).ConfigureAwait(false);
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImagesToAlbumAsync(UploadToAlbumRequest request, CancellationToken ct = default)
    {
        var uploadedFiles = await Upload(request, ct).ToListAsync(ct).ConfigureAwait(false);

        return await client.ModifyAlbumAsync(new ModifyAlbumImagesRequest
        {
            Request = request.Request,
            UserHash = request.UserHash,
            AlbumId = request.AlbumId,
            Files = uploadedFiles
        }, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<AlbumUploadResult> UploadImagesToAlbumSafeAsync(UploadToAlbumRequest request, CancellationToken ct = default)
    {
        // Get album to determine current capacity
        var album = await GetAlbumAsync(request.AlbumId, ct).ConfigureAwait(false);
        var currentCount = album.Files.Length;
        var remainingCapacity = Common.MaxAlbumFiles - currentCount;

        // Count total files in request (materializes lazy enumerables)
        var (totalFiles, materializedRequest) = MaterializeAndCountRequest(request);

        // If album is full, return early
        if (remainingCapacity <= 0)
        {
            return new AlbumUploadResult
            {
                AlbumUrl = null,
                FilesUploaded = 0,
                FilesSkipped = totalFiles,
                RemainingCapacity = 0
            };
        }

        // Calculate how many to upload
        var filesToUpload = Math.Min(remainingCapacity, totalFiles);
        var filesToSkip = totalFiles - filesToUpload;

        // If we need to upload all files, use the original request
        // Otherwise, create a sliced request
        var requestToUse = filesToUpload == totalFiles
            ? materializedRequest
            : TakeFilesFromRequest(materializedRequest, filesToUpload);

        // Upload and add to album
        var result = await UploadImagesToAlbumAsync(requestToUse, ct).ConfigureAwait(false);

        return new AlbumUploadResult
        {
            AlbumUrl = result,
            FilesUploaded = filesToUpload,
            FilesSkipped = filesToSkip,
            RemainingCapacity = remainingCapacity - filesToUpload
        };
    }

    /// <inheritdoc/>
    public async Task<AlbumInfo> GetAlbumAsync(string albumId, CancellationToken ct = default)
    {
        return await client.GetAlbumAsync(new GetAlbumRequest { AlbumId = albumId }, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DownloadFileAsync(string fileName, DirectoryInfo destination, CancellationToken ct = default)
    {
        await client.DownloadFileAsync(fileName, destination, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DownloadFileAsync(string fileName, [StringSyntax(StringSyntaxAttribute.Uri)] string destinationPath, CancellationToken ct = default)
    {
        await client.DownloadFileAsync(fileName, destinationPath, ct).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<FileInfo> DownloadAlbumAsync(string albumId, DirectoryInfo destination, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var album = await GetAlbumAsync(albumId, ct).ConfigureAwait(false);

        if (!destination.Exists)
            destination.Create();

        foreach (var fileName in album.Files)
        {
            ct.ThrowIfCancellationRequested();

            var filePath = Path.Combine(destination.FullName, fileName);

            await client.DownloadFileAsync(fileName, destination, ct).ConfigureAwait(false);

            yield return new FileInfo(filePath);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<FileInfo> DownloadAlbumAsync(string albumId, [StringSyntax(StringSyntaxAttribute.Uri)] string destinationPath, CancellationToken ct = default)
    {
        return DownloadAlbumAsync(albumId, new DirectoryInfo(destinationPath), ct);
    }

    /// <summary>
    /// Upload files based on the requestBase type
    /// </summary>
    /// <param name="request">Upload requestBase type</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>API Response</returns>
    /// <exception cref="InvalidOperationException">When passing in an invalid requestBase type</exception>
    private IAsyncEnumerable<string?> Upload(IAlbumUploadRequest request, CancellationToken ct = default)
    {
        return request.UploadRequest switch
        {
            { IsFirst: true } => client.UploadFilesAsync(request.UploadRequest, ct),
            { IsSecond: true } => client.UploadFilesAsStreamAsync(request.UploadRequest.Second, ct),
            { IsThird: true } => client.UploadFilesAsUrlAsync(request.UploadRequest, ct),
            _ => throw new InvalidOperationException("Invalid requestBase type")
        };
    }

    /// <summary>
    /// Materializes lazy enumerables and counts total files in the request
    /// </summary>
    private static (int Count, UploadToAlbumRequest MaterializedRequest) MaterializeAndCountRequest(UploadToAlbumRequest request)
    {
        return request.UploadRequest switch
        {
            { IsFirst: true } => MaterializeFileUploadRequest(request),
            { IsSecond: true } => MaterializeStreamUploadRequest(request),
            { IsThird: true } => MaterializeUrlUploadRequest(request),
            _ => throw new InvalidOperationException("Invalid request type")
        };

        static (int, UploadToAlbumRequest) MaterializeFileUploadRequest(UploadToAlbumRequest req)
        {
            var files = req.UploadRequest.First.Files.ToList();
            var materialized = req with
            {
                UploadRequest = req.UploadRequest.First with { Files = files }
            };
            return (files.Count, materialized);
        }

        static (int, UploadToAlbumRequest) MaterializeStreamUploadRequest(UploadToAlbumRequest req)
        {
            var streams = req.UploadRequest.Second.ToList();
            var materialized = req with { UploadRequest = streams };
            return (streams.Count, materialized);
        }

        static (int, UploadToAlbumRequest) MaterializeUrlUploadRequest(UploadToAlbumRequest req)
        {
            var urls = req.UploadRequest.Third.Files.ToList();
            var materialized = req with
            {
                UploadRequest = req.UploadRequest.Third with { Files = urls }
            };
            return (urls.Count, materialized);
        }
    }

    /// <summary>
    /// Creates a new request with only the first N files
    /// </summary>
    private static UploadToAlbumRequest TakeFilesFromRequest(UploadToAlbumRequest request, int count)
    {
        return request.UploadRequest switch
        {
            { IsFirst: true } => request with
            {
                UploadRequest = request.UploadRequest.First with { Files = request.UploadRequest.First.Files.Take(count) }
            },
            { IsSecond: true } => request with
            {
                UploadRequest = request.UploadRequest.Second.Take(count).ToList()
            },
            { IsThird: true } => request with
            {
                UploadRequest = request.UploadRequest.Third with { Files = request.UploadRequest.Third.Files.Take(count) }
            },
            _ => throw new InvalidOperationException("Invalid request type")
        };
    }
}