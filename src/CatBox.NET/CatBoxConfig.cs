namespace CatBox.NET;

public record CatBoxConfig
{
    public Uri CatBoxUrl { get; set; }
    public Uri LitterboxUrl { get; set; }
}