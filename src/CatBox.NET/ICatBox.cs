namespace CatBox.NET;


/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
/// <remarks>Not currently implemented so don't use</remarks>
public interface ICatBox
{
    public Task UploadFile();
    public Task DeleteFile();
    public Task CreateAlbum();
    public Task EditAlbum();
    public Task AddToAlbum();
    public Task RemoveFromAlbum();
    public Task DeleteAlbum();
}
