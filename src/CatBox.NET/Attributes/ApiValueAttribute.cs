using System.Diagnostics.CodeAnalysis;

namespace CatBox.NET.Client.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ApiValueAttribute : Attribute
{
    /// <summary>
    /// Specifies the default value for the <see cref='System.ComponentModel.DescriptionAttribute'/>,
    /// which is an empty string (""). This <see langword='static'/> field is read-only.
    /// </summary>
    public static readonly ApiValueAttribute Default = new(string.Empty);

    /// <summary>
    /// Initializes a new instance of the <see cref='System.ComponentModel.DescriptionAttribute'/> class.
    /// </summary>
    /// <param name="apiValue">Represents the CatBox API verb or parameter value</param>
    public ApiValueAttribute(string apiValue)
    {
        ApiValue = apiValue;
    }

    /// <summary>
    /// Read/Write property that directly modifies the string stored in the description
    /// attribute. The default implementation of the <see cref="ApiValue"/> property
    /// simply returns this value.
    /// </summary>
    public string ApiValue { get; set; }

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is ApiValueAttribute other && other.ApiValue == ApiValue;

    public override int GetHashCode() => ApiValue?.GetHashCode() ?? 0;

    public override bool IsDefaultAttribute() => Equals(Default);
}