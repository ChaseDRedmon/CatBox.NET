namespace CatBox.NET.Client;


/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
/// <remarks>Not currently implemented so don't use</remarks>
public interface ICatBox
{
    Task UploadFile();
    Task DeleteFile();
    Task CreateAlbum();
    Task EditAlbum();
    Task AddToAlbum();
    Task RemoveFromAlbum();
    Task DeleteAlbum();
}
