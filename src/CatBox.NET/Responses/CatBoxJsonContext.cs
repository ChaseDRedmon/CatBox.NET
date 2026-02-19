using System.Text.Json.Serialization;
using CatBox.NET.Responses.Album;

namespace CatBox.NET.Responses;

[JsonSerializable(typeof(GetAlbumApiResponse))]
[JsonSerializable(typeof(CatBoxApiErrorResponse))]
internal sealed partial class CatBoxJsonContext : JsonSerializerContext;
