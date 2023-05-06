namespace CatBox.NET.Exceptions;

internal class CatBoxFileNotFoundException : Exception
{
    public override string Message { get; } = "The CatBox File was not found";
}

internal class CatBoxAlbumNotFoundException : Exception
{
    public override string Message { get; } = "The CatBox Album was not found";
}