using Intellenum;

namespace CatBox.NET.Enums;

/// <summary>
/// API Request parameters for requestBase content and requestBase types
/// </summary>
[Intellenum<string>(Conversions.TypeConverter)]
[Member("Request", "reqtype")]
[Member("UserHash", "userhash")]
[Member("Url", "url")]
[Member("Files", "files")]
[Member("FileToUpload", "fileToUpload")]
[Member("Title", "title")]
[Member("Description", "desc")]
[Member("AlbumIdShort", "short")]
[Member("Expiry", "time")]
internal sealed partial class RequestParameters;