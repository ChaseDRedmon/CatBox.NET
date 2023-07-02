using BetterEnumsGen;
using CatBox.NET.Client.Attributes;

namespace CatBox.NET.Enums;

/// <summary>
/// Image expiry in litterbox.moe
/// </summary>
[BetterEnum]
public enum ExpireAfter
{
    /// <summary>
    /// Expire after 1 hour
    /// </summary>
    [ApiValue("1h")]
    OneHour,
    
    /// <summary>
    /// Expire after 12 hours
    /// </summary>
    [ApiValue("12h")]
    TwelveHours,
    
    /// <summary>
    /// Expire after one day (24 hours)
    /// </summary>
    [ApiValue("24h")]
    OneDay,
    
    /// <summary>
    /// Expire after three days (72 hours)
    /// </summary>
    [ApiValue("72h")]
    ThreeDays
}
