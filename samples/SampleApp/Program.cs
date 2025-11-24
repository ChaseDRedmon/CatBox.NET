using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using CatBox.NET.Requests.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Load configuration from user secrets
// To set your UserHash, run: dotnet user-secrets set "CatBox:UserHash" "your-hash-here" --project samples/SampleApp
// Get your UserHash from: https://catbox.moe/user/manage.php
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var userHash = configuration["CatBox:UserHash"]
    ?? throw new InvalidOperationException(
        "CatBox UserHash not configured. Run: dotnet user-secrets set \"CatBox:UserHash\" \"your-hash-here\" --project samples/SampleApp");

// Compute path to test image file within the project directory
string testImagePath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "..", "..", "..", "..", "..",  // Navigate up from bin/Debug/net7.0
    "tests", "CatBox.Tests", "Images", "test-file.png"
);
testImagePath = Path.GetFullPath(testImagePath); // Normalize the path
string testImageFileName = Path.GetFileName(testImagePath);

// Verify the test image file exists
if (!File.Exists(testImagePath))
{
    throw new FileNotFoundException($"Test image not found at: {testImagePath}");
}

var collection = new ServiceCollection()
    .AddCatBoxServices(f =>
    {
        f.CatBoxUrl = new Uri("https://catbox.moe/user/api.php");
        f.LitterboxUrl = new Uri("https://litterbox.catbox.moe/resources/internals/api.php");
    })
    .AddLogging(f => f.AddConsole())
    .BuildServiceProvider();

// Store uploaded file URLs and album URL for cleanup
var uploadedFiles = new List<string>();
string? albumUrl = null;

// Upload a single image via stream
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var responses = client.UploadFilesAsStreamAsync([new StreamUploadRequest
    {
        Stream = File.OpenRead(testImagePath),
        FileName = testImageFileName,
        UserHash = userHash
    }]);

    await foreach (var response in responses)
    {
        Console.WriteLine(response);
        if (!string.IsNullOrWhiteSpace(response))
            uploadedFiles.Add(response);
    }
}

// Create an album of images already on Catbox
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var response = await client.CreateAlbumAsync(new RemoteCreateAlbumRequest
    {
        Title = "Album Title",
        Description = "Album Description",
        Files = uploadedFiles, // Use the actual uploaded file(s) from previous step
        UserHash = userHash
    });

    Console.WriteLine(response);
    albumUrl = response;
}

// Cleanup: Delete the album and uploaded files
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    await CleanupAsync(client, albumUrl, uploadedFiles, userHash);
}

return;

/*// Upload images to CatBox, then create an album on CatBox, then place the uploaded images into the newly created album
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ICatBox>();
    var response = await client.CreateAlbumFromFilesAsync(new CreateAlbumRequest
    {
        Title = "Album Title",
        Description = "Album Description",
        UserHash = null,
        UploadRequest = new FileUploadRequest
        {
            Files = [new FileInfo(testImagePath)]
        }
    });

    Console.WriteLine(response);
}

// Temporarily upload an image to litterbox
using (var scope = collection.CreateScope())
{
    var client = scope.ServiceProvider.GetRequiredService<ILitterboxClient>();
    var response = await client.UploadImageAsync(new TemporaryStreamUploadRequest
    {
        Expiry = ExpireAfter.OneHour,
        FileName = testImageFileName,
        Stream = File.OpenRead(testImagePath)
    });

    Console.WriteLine(response);

    Console.WriteLine();
}

Console.ReadLine();*/

// Cleanup method to delete album and uploaded files
static async Task CleanupAsync(ICatBoxClient client, string? albumUrl, List<string> uploadedFiles, string userHash)
{
    Console.WriteLine("\n--- Starting Cleanup ---");

    // Delete the album first
    if (!string.IsNullOrWhiteSpace(albumUrl))
    {
        try
        {
            // Extract album short ID from URL (e.g., "pd412w" from "https://catbox.moe/c/pd412w")
            var albumUri = new Uri(albumUrl);
            var albumId = albumUri.AbsolutePath.TrimStart('/').Replace("c/", "");

            Console.WriteLine($"Deleting album: {albumId}");

            var albumDeleteResponse = await client.ModifyAlbumAsync(new ModifyAlbumImagesRequest
            {
                Request = RequestType.DeleteAlbum,
                UserHash = userHash,
                AlbumId = albumId,
                Files = [] // Empty for DeleteAlbum operation
            });

            Console.WriteLine($"Album deletion response: {albumDeleteResponse}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting album: {ex.Message}");
        }
    }

    // Delete the uploaded files
    if (uploadedFiles.Count > 0)
    {
        try
        {
            // Extract filenames from URLs (e.g., "8ce67f.jpg" from "https://files.catbox.moe/8ce67f.jpg")
            var fileNames = uploadedFiles.Select(url =>
            {
                var uri = new Uri(url);
                return Path.GetFileName(uri.AbsolutePath);
            }).ToList();

            Console.WriteLine($"Deleting {fileNames.Count} file(s): {string.Join(", ", fileNames)}");

            var fileDeleteResponse = await client.DeleteMultipleFilesAsync(new DeleteFileRequest
            {
                UserHash = userHash,
                FileNames = fileNames
            });

            Console.WriteLine($"File deletion response: {fileDeleteResponse}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting files: {ex.Message}");
        }
    }

    Console.WriteLine("--- Cleanup Complete ---\n");
}
