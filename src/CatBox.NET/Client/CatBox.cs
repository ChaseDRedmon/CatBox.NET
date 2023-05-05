namespace CatBox.NET.Client;

[Obsolete("Do not use at this time")]
public sealed class Catbox : ICatBox
{
    private readonly ICatBoxClient _client;

    /// <summary>
    /// Instantiate a new catbox class 
    /// </summary>
    /// <param name="userHash"></param>
    /// <param name="host"></param>
    /// <param name="client"></param>
    public Catbox(ICatBoxClient client)
    {
        _client = client;
    }

    public Task UploadFile()
    {
        throw new NotImplementedException();
    }

    public Task DeleteFile()
    {
        throw new NotImplementedException();
    }

    public Task CreateAlbum()
    {
        throw new NotImplementedException();
    }

    public Task EditAlbum()
    {
        throw new NotImplementedException();
    }

    public Task AddToAlbum()
    {
        throw new NotImplementedException();
    }

    public Task RemoveFromAlbum()
    {
        throw new NotImplementedException();
    }

    public Task DeleteAlbum()
    {
        throw new NotImplementedException();
    }
}