namespace Meatcorps.Engine.Core.Extensions;

public static class StringExtensions
{
    public static string ToCapitalize(this string s) 
    {
        return string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
    }
}