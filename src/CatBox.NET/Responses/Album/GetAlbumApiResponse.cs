using System.Text.Json.Serialization;

namespace CatBox.NET.Responses.Album;

/// <summary>
/// Raw API response from getalbum endpoint
/// </summary>
internal sealed record GetAlbumApiResponse
{
    [JsonPropertyName("data")]
    public required AlbumApiData? Data { get; init; }

    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("status")]
    public int Status { get; init; }
}

internal sealed record AlbumApiData
{
    [JsonPropertyName("files")]
    public required string Files { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("short")]
    public required string Short { get; init; }

    [JsonPropertyName("datecreated")]
    public required DateOnly DateCreated { get; init; }
}
