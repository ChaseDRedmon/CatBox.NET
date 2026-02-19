using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.Litterbox;
using CatBox.NET.Requests.URL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Load configuration from user secrets (optional — anonymous uploads work without it)
// To set your UserHash, run: dotnet user-secrets set "CatBox:UserHash" "your-hash-here" --project samples/SampleApp
// Get your UserHash from: https://catbox.moe/user/manage.php
string? userHash;
try
{
    var configuration = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    userHash = configuration["CatBox:UserHash"];
}
catch
{
    userHash = null;
}

var collection = new ServiceCollection()
    .AddCatBoxServices(f =>
    {
        f.CatBoxUrl = new Uri("https://catbox.moe/user/api.php");
        f.LitterboxUrl = new Uri("https://litterbox.catbox.moe/resources/internals/api.php");
    })
    .AddLogging(f => f.AddConsole().SetMinimumLevel(LogLevel.Warning))
    .BuildServiceProvider();

// Track uploaded file URLs and album URLs across operations
var trackedFiles = new List<string>();
var trackedAlbums = new List<string>();

Console.WriteLine("=== CatBox.NET Interactive Sample ===");
if (string.IsNullOrWhiteSpace(userHash))
    Console.WriteLine("[Warning] UserHash not configured. Some operations will be unavailable.");
else
    Console.WriteLine($"[OK] UserHash configured.");
Console.WriteLine();

while (true)
{
    PrintMenu();
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();

    if (!int.TryParse(input, out var choice))
    {
        Console.WriteLine("Invalid input. Enter a number.\n");
        continue;
    }

    try
    {
        switch (choice)
        {
            case 0:
                Console.WriteLine("Goodbye!");
                return;
            case 1:
                await UploadFilesFromDisk();
                break;
            case 2:
                await UploadFilesAsStream();
                break;
            case 3:
                await UploadFilesByUrl();
                break;
            case 4:
                await DeleteFiles();
                break;
            case 5:
                await CreateAlbumFromTracked();
                break;
            case 6:
                await CreateAlbumAndUpload();
                break;
            case 7:
                await EditAlbum();
                break;
            case 8:
                await ModifyAlbum();
                break;
            case 9:
                await GetAlbumInfo();
                break;
            case 10:
                await DownloadFile();
                break;
            case 11:
                await DownloadAlbum();
                break;
            case 12:
                await LitterboxUploadFromDisk();
                break;
            case 13:
                await LitterboxUploadAsStream();
                break;
            case 14:
                ShowTrackedItems();
                break;
            default:
                Console.WriteLine("Unknown option.\n");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n");
    }
}

// --- Helpers ---

static void PrintMenu()
{
    Console.WriteLine("""
    --- File Operations ---
    [1] Upload file(s) from disk
    [2] Upload file(s) as stream
    [3] Upload file(s) by URL
    [4] Delete file(s)

    --- Album Operations ---
    [5] Create album (from tracked files)
    [6] Create album + upload files (one step)
    [7] Edit album
    [8] Add/Remove files or Delete album
    [9] Get album info

    --- Download ---
    [10] Download a file
    [11] Download entire album

    --- Litterbox (Temporary) ---
    [12] Upload temporary file(s) from disk
    [13] Upload temporary file as stream

    --- Tracked Items ---
    [14] Show tracked files & albums
    [0] Exit
    
    """);
}

static string ExtractFileName(string url)
{
    if (url.Contains("files.catbox.moe"))
        return url.Split('/')[^1];
    return url;
}

static string ExtractAlbumShortCode(string url)
{
    if (url.Contains("catbox.moe/c/"))
        return url.Split('/')[^1];
    return url;
}

static ExpireAfter PromptExpiry()
{
    Console.WriteLine("Select expiry:");
    Console.WriteLine("  [1] 1 hour");
    Console.WriteLine("  [2] 12 hours");
    Console.WriteLine("  [3] 24 hours (1 day)");
    Console.WriteLine("  [4] 72 hours (3 days)");
    Console.Write("Choice: ");
    var expiryInput = Console.ReadLine()?.Trim();
    return expiryInput switch
    {
        "2" => ExpireAfter.TwelveHours,
        "3" => ExpireAfter.OneDay,
        "4" => ExpireAfter.ThreeDays,
        _ => ExpireAfter.OneHour,
    };
}

bool RequireUserHash()
{
    if (!string.IsNullOrWhiteSpace(userHash))
        return true;

    Console.WriteLine("This operation requires a UserHash. Configure one first.");
    Console.WriteLine("Run: dotnet user-secrets set \"CatBox:UserHash\" \"your-hash-here\" --project samples/SampleApp\n");
    return false;
}

// --- Operations ---

async Task UploadFilesFromDisk()
{
    Console.Write("Enter file path(s) (comma-separated): ");
    var paths = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (paths is null || paths.Length == 0) return;

    var files = paths.Select(p => new FileInfo(Path.GetFullPath(p))).ToList();
    foreach (var f in files.Where(f => !f.Exists))
        Console.WriteLine($"  Warning: file not found: {f.FullName}");

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var request = new FileUploadRequest { Files = files, UserHash = userHash };

    await foreach (var result in client.UploadFilesAsync(request))
    {
        Console.WriteLine($"  Uploaded: {result}");
        if (!string.IsNullOrWhiteSpace(result))
            trackedFiles.Add(result);
    }
    Console.WriteLine();
}

async Task UploadFilesAsStream()
{
    Console.Write("Enter file path: ");
    var path = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(path)) return;
    path = Path.GetFullPath(path);

    if (!File.Exists(path))
    {
        Console.WriteLine($"  File not found: {path}\n");
        return;
    }

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var stream = File.OpenRead(path);

    var requests = new[]
    {
        new StreamUploadRequest
        {
            Stream = stream,
            FileName = Path.GetFileName(path),
            UserHash = userHash
        }
    };

    await foreach (var result in client.UploadFilesAsStreamAsync(requests))
    {
        Console.WriteLine($"  Uploaded: {result}");
        if (!string.IsNullOrWhiteSpace(result))
            trackedFiles.Add(result);
    }
    Console.WriteLine();
}

async Task UploadFilesByUrl()
{
    Console.Write("Enter URL(s) (comma-separated): ");
    var urls = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (urls is null || urls.Length == 0) return;

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var request = new UrlUploadRequest
    {
        Files = urls.Select(u => new Uri(u)),
        UserHash = userHash
    };

    await foreach (var result in client.UploadFilesAsUrlAsync(request))
    {
        Console.WriteLine($"  Uploaded: {result}");
        if (!string.IsNullOrWhiteSpace(result))
            trackedFiles.Add(result);
    }
    Console.WriteLine();
}

async Task DeleteFiles()
{
    if (!RequireUserHash()) return;

    if (trackedFiles.Count > 0)
    {
        Console.WriteLine("Tracked files:");
        for (var i = 0; i < trackedFiles.Count; i++)
            Console.WriteLine($"  [{i}] {trackedFiles[i]}");
    }

    Console.Write("Enter CatBox file name(s) to delete (comma-separated, e.g., abc123.png): ");
    var names = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (names is null || names.Length == 0) return;

    var fileNames = names.Select(ExtractFileName).ToList();

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var result = await client.DeleteMultipleFilesAsync(new DeleteFileRequest
    {
        UserHash = userHash!,
        FileNames = fileNames
    });

    Console.WriteLine($"  Delete result: {result}");

    // Remove deleted files from tracking
    trackedFiles.RemoveAll(f => fileNames.Contains(ExtractFileName(f)));
    Console.WriteLine();
}

async Task CreateAlbumFromTracked()
{
    if (trackedFiles.Count == 0)
    {
        Console.WriteLine("No tracked files. Upload some files first.\n");
        return;
    }

    Console.WriteLine("Tracked files:");
    for (var i = 0; i < trackedFiles.Count; i++)
        Console.WriteLine($"  [{i}] {trackedFiles[i]}");

    Console.Write("Enter indices to include (comma-separated, or 'all'): ");
    var selection = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(selection)) return;

    List<string> selectedFiles;
    if (selection.Equals("all", StringComparison.OrdinalIgnoreCase))
    {
        selectedFiles = trackedFiles.ToList();
    }
    else
    {
        var indices = selection.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .Where(i => i >= 0 && i < trackedFiles.Count);
        selectedFiles = indices.Select(i => trackedFiles[i]).ToList();
    }

    if (selectedFiles.Count == 0)
    {
        Console.WriteLine("No valid files selected.\n");
        return;
    }

    Console.Write("Album title: ");
    var title = Console.ReadLine()?.Trim() ?? "Untitled";
    Console.Write("Album description (optional): ");
    var description = Console.ReadLine()?.Trim();

    // Extract just the file names from URLs
    var fileNames = selectedFiles.Select(ExtractFileName).ToList();

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var result = await client.CreateAlbumAsync(new RemoteCreateAlbumRequest
    {
        Title = title,
        Description = description,
        UserHash = userHash,
        Files = fileNames
    });

    Console.WriteLine($"  Album created: {result}");
    if (!string.IsNullOrWhiteSpace(result))
        trackedAlbums.Add(result);
    Console.WriteLine();
}

async Task CreateAlbumAndUpload()
{
    Console.Write("Enter file path(s) to upload (comma-separated): ");
    var paths = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (paths is null || paths.Length == 0) return;

    Console.Write("Album title: ");
    var title = Console.ReadLine()?.Trim() ?? "Untitled";
    Console.Write("Album description (optional): ");
    var description = Console.ReadLine()?.Trim();

    var files = paths.Select(p => new FileInfo(Path.GetFullPath(p))).ToList();

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBox>();
    var result = await client.CreateAlbumFromFilesAsync(new CreateAlbumRequest
    {
        Title = title,
        Description = description,
        UserHash = userHash,
        UploadRequest = new FileUploadRequest { Files = files, UserHash = userHash }
    });

    Console.WriteLine($"  Album created: {result}");
    if (!string.IsNullOrWhiteSpace(result))
        trackedAlbums.Add(result);
    Console.WriteLine();
}

async Task EditAlbum()
{
    if (!RequireUserHash()) return;

    Console.Write("Enter album ID or URL: ");
    var albumInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(albumInput)) return;
    var albumId = ExtractAlbumShortCode(albumInput);

    Console.Write("New title: ");
    var title = Console.ReadLine()?.Trim() ?? "Untitled";
    Console.Write("New description: ");
    var description = Console.ReadLine()?.Trim() ?? "";

    Console.Write("Enter file name(s) for the album (comma-separated): ");
    var fileInput = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (fileInput is null || fileInput.Length == 0)
    {
        Console.WriteLine("No files specified.\n");
        return;
    }

    var fileNames = fileInput.Select(ExtractFileName).ToList();

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();

#pragma warning disable CS0618 // EditAlbumAsync is marked Obsolete as a safety warning
    var result = await client.EditAlbumAsync(new EditAlbumRequest
    {
        UserHash = userHash!,
        AlbumId = albumId,
        Title = title,
        Description = description,
        Files = fileNames
    });
#pragma warning restore CS0618

    Console.WriteLine($"  Edit result: {result}\n");
}

async Task ModifyAlbum()
{
    if (!RequireUserHash()) return;

    Console.WriteLine("  [1] Add files to album");
    Console.WriteLine("  [2] Remove files from album");
    Console.WriteLine("  [3] Delete album");
    Console.Write("Choice: ");
    var subChoice = Console.ReadLine()?.Trim();

    Console.Write("Enter album ID or URL: ");
    var albumInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(albumInput)) return;
    var albumId = ExtractAlbumShortCode(albumInput);

    RequestType requestType;
    List<string> fileNames = [];

    switch (subChoice)
    {
        case "1":
            requestType = RequestType.AddToAlbum;
            Console.Write("Enter file name(s) to add (comma-separated): ");
            var addInput = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (addInput is null || addInput.Length == 0) return;
            fileNames = addInput.Select(ExtractFileName).ToList();
            break;
        case "2":
            requestType = RequestType.RemoveFromAlbum;
            Console.Write("Enter file name(s) to remove (comma-separated): ");
            var removeInput = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (removeInput is null || removeInput.Length == 0) return;
            fileNames = removeInput.Select(ExtractFileName).ToList();
            break;
        case "3":
            requestType = RequestType.DeleteAlbum;
            break;
        default:
            Console.WriteLine("Invalid choice.\n");
            return;
    }

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    var result = await client.ModifyAlbumAsync(new ModifyAlbumImagesRequest
    {
        Request = requestType,
        UserHash = userHash!,
        AlbumId = albumId,
        Files = fileNames
    });

    Console.WriteLine($"  Result: {result}");

    if (requestType == RequestType.DeleteAlbum)
        trackedAlbums.RemoveAll(a => ExtractAlbumShortCode(a) == albumId);
    Console.WriteLine();
}

async Task GetAlbumInfo()
{
    Console.Write("Enter album ID or URL: ");
    var albumInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(albumInput)) return;
    var albumId = ExtractAlbumShortCode(albumInput);

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBox>();
    var info = await client.GetAlbumAsync(albumId);

    Console.WriteLine($"  Title:       {info.Title}");
    Console.WriteLine($"  Description: {info.Description}");
    Console.WriteLine($"  Album ID:    {info.AlbumId}");
    Console.WriteLine($"  Created:     {info.DateCreated}");
    Console.WriteLine($"  Files ({info.Files.Length}):");
    foreach (var file in info.Files)
        Console.WriteLine($"    - {file}");
    Console.WriteLine();
}

async Task DownloadFile()
{
    Console.Write("Enter CatBox file name (e.g., abc123.png): ");
    var fileName = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(fileName)) return;
    fileName = ExtractFileName(fileName);

    Console.Write("Enter destination directory: ");
    var destDir = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(destDir)) return;

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBoxClient>();
    await client.DownloadFileAsync(fileName, destDir);

    var filePath = Path.Combine(destDir, fileName);
    Console.WriteLine($"  Downloaded: {filePath}\n");
}

async Task DownloadAlbum()
{
    Console.Write("Enter album ID or URL: ");
    var albumInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(albumInput)) return;
    var albumId = ExtractAlbumShortCode(albumInput);

    Console.Write("Enter destination directory: ");
    var destDir = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(destDir)) return;

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ICatBox>();

    Console.WriteLine("  Downloading album files...");
    await foreach (var fileInfo in client.DownloadAlbumAsync(albumId, destDir))
    {
        Console.WriteLine($"  Downloaded: {fileInfo.FullName}");
    }
    Console.WriteLine();
}

async Task LitterboxUploadFromDisk()
{
    Console.Write("Enter file path(s) (comma-separated): ");
    var paths = Console.ReadLine()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (paths is null || paths.Length == 0) return;

    var expiry = PromptExpiry();
    var files = paths.Select(p => new FileInfo(Path.GetFullPath(p))).ToList();

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ILitterboxClient>();

    await foreach (var result in client.UploadMultipleImagesAsync(new TemporaryFileUploadRequest
    {
        Files = files,
        Expiry = expiry
    }))
    {
        Console.WriteLine($"  Uploaded (temporary): {result}");
    }
    Console.WriteLine();
}

async Task LitterboxUploadAsStream()
{
    Console.Write("Enter file path: ");
    var path = Console.ReadLine()?.Trim();
    if (string.IsNullOrWhiteSpace(path)) return;
    path = Path.GetFullPath(path);
    if (!File.Exists(path))
    {
        Console.WriteLine("  File not found.\n");
        return;
    }

    var expiry = PromptExpiry();

    using var scope = collection.CreateScope();
    var client = scope.ServiceProvider.GetRequiredService<ILitterboxClient>();
    var result = await client.UploadImageAsync(new TemporaryStreamUploadRequest
    {
        FileName = Path.GetFileName(path),
        Stream = File.OpenRead(path),
        Expiry = expiry
    });

    Console.WriteLine($"  Uploaded (temporary): {result}\n");
}

void ShowTrackedItems()
{
    Console.WriteLine($"--- Tracked Files ({trackedFiles.Count}) ---");
    for (var i = 0; i < trackedFiles.Count; i++)
        Console.WriteLine($"  [{i}] {trackedFiles[i]}");

    Console.WriteLine($"--- Tracked Albums ({trackedAlbums.Count}) ---");
    for (var i = 0; i < trackedAlbums.Count; i++)
        Console.WriteLine($"  [{i}] {trackedAlbums[i]}");

    Console.WriteLine();
}
