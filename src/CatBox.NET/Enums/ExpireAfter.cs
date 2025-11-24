using Intellenum;

namespace CatBox.NET.Enums;

/// <summary>
/// Image expiry in litterbox.moe
/// </summary>
[Intellenum<string>(Conversions.TypeConverter)]
[Member("OneHour", "1h")]
[Member("TwelveHours", "12h")]
[Member("OneDay", "24h")]
[Member("ThreeDays", "72h")]
public sealed partial class ExpireAfter;
