using CatBox.NET.Requests;

namespace CatBox.NET.Client;

public interface ILitterboxClient
{
    /// <summary>
    /// Enables uploading multiple files from disk (FileStream) to the API
    /// </summary>
    /// <param name="temporaryFileUploadRequest"></param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ArgumentNullException">When <see cref="FileUploadRequest"/> is null</exception>
    /// <returns>Response string from the API</returns>
    IAsyncEnumerable<string?> UploadMultipleImages(TemporaryFileUploadRequest temporaryFileUploadRequest, CancellationToken ct = default);

    /// <summary>
    /// Streams a single image to be uploaded
    /// </summary>
    /// <param name="temporaryStreamUploadRequest"></param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ArgumentNullException">When <see cref="StreamUploadRequest"/> is null</exception>
    /// <exception cref="ArgumentNullException">When <see cref="StreamUploadRequest.FileName"/> is null</exception>
    /// <exception cref="HttpRequestException"> when something bad happens when talking to the API</exception>
    /// <returns>Response string from the API</returns>
    Task<string?> UploadImage(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default);
}
