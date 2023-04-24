// See https://aka.ms/new-console-template for more information

using CatBox.NET;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var collection = new ServiceCollection()
    .AddCatBoxServices(f => f.CatBoxUrl = new Uri("https://catbox.moe/user/api.php"))
    .AddLogging(f => f.AddSerilog(dispose: true))
    .BuildServiceProvider();

// Upload a single image
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var response = await client.UploadImage(new StreamUploadRequest
    {
        Stream = File.OpenRead(@"C:\Users\redmo\Documents\Anime\13c9a4.png"),
        FileName = Path.GetFileName(@"C:\Users\redmo\Documents\Anime\13c9a4.png")
    });

    Console.WriteLine(response);
}

// Create an album of images already on Catbox
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var response = await client.CreateAlbum(new CreateAlbumRequest
    {
        Title = "Album Title",
        Description = "Album Description",
        Files = new [] { "1.jpg", }
    });

    Console.WriteLine(response);
}

// Temporarily upload an image to litterbox
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ILitterboxClient>();
    var response = await client.UploadImage(new TemporaryStreamUploadRequest
    {
        ExpireAfter = ExpireAfter.OneHour,
        FileName = Path.GetFileName(@"C:\Users\redmo\Documents\Anime\13c9a4.png"),
        Stream = File.OpenRead(@"C:\Users\redmo\Documents\Anime\13c9a4.png")
    });

    Console.WriteLine(response);
}

Console.ReadLine();