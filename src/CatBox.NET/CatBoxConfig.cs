namespace CatBox.NET;

public record CatBoxConfig
{
    public Uri CatBoxUrl { get; init; }
    public Uri LitterboxUrl { get; init; }
}