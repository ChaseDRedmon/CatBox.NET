using CatBox.NET;
using Microsoft.Extensions.Options;

namespace CatBox.Tests.Unit;

internal class Helper
{
    public static IOptions<CatBoxConfig> GetCatBoxConfig(Uri catboxUrl, Uri litterboxUrl)
    {
        return Options.Create(
            new CatBoxConfig
            {
                CatBoxUrl = catboxUrl,
                LitterboxUrl = litterboxUrl
            }
        );
    }
}