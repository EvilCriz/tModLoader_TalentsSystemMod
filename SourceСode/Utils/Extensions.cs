using System;
using System.Globalization;

namespace TalentsSystem.Utils;

/// <summary>
///     Utility with extension methods
/// </summary>
public static class UtilsExtensions
{
    /// <summary>
    ///     A <see cref="float.ToString"/> equivalent that accounts for float calculation error
    /// </summary>
    public static string ToStringF(this float value, int count = 5, bool invariantCulture = true)
    {
        if(count > 0)
            value += (1f / (float)Math.Pow(10, count + 2));
        string s = "0.";
        for (; count > 0; count--)
            s += "#";
        return invariantCulture
            ? value.ToString(s, CultureInfo.InvariantCulture)
            : value.ToString(s);
    }
}
