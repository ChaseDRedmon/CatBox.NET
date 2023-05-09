namespace CatBox.NET.Exceptions;

internal sealed class CatBoxFileNotFoundException : Exception
{
    public override string Message { get; } = "The CatBox File was not found";
}

internal sealed class CatBoxAlbumNotFoundException : Exception
{
    public override string Message { get; } = "The CatBox Album was not found";
}

// API Response Message: No request type given.
internal sealed class CatBoxMissingRequestTypeException : Exception
{
    public override string Message { get; } = "The CatBox Request Type was not specified. Did you miss an API parameter?";
}

// API Response Message: No files given.
internal sealed class CatBoxMissingFileException : Exception
{
    public override string Message { get; } = "The FileToUpload parameter was not specified or is missing content. Did you miss an API parameter?";
}

//API Response Message: No expire time specified.
internal sealed class LitterboxInvalidExpiry : Exception
{
    public override string Message { get; } = "The Litterbox expiry request parameter is invalid. Valid expiration times are: 1h, 12h, 24h, 72h";
}