namespace CatBox.NET;

/// <summary>
/// A base record for all file upload requests where the UserHash is optional
/// </summary>
public abstract record UploadRequest
{
    /// <summary>
    /// The UserHash associated with this file upload
    /// </summary>
    public string? UserHash { get; init; }
}