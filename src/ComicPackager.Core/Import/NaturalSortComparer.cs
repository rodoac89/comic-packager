using System.Globalization;

namespace ComicPackager.Core.Import;

/// <summary>
/// Orden natural: "página 2.jpg" antes que "página 10.jpg".
/// Compara tramos numéricos por valor (sin overflow) y el resto de forma ordinal ignorando mayúsculas.
/// </summary>
public sealed class NaturalSortComparer : IComparer<string>
{
    public static NaturalSortComparer Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (x is null)
            return -1;
        if (y is null)
            return 1;

        var ix = 0;
        var iy = 0;
        while (ix < x.Length && iy < y.Length)
        {
            if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
            {
                var cmp = CompareNumbers(x, ref ix, y, ref iy);
                if (cmp != 0)
                    return cmp;
                continue;
            }

            var cx = char.ToUpper(x[ix], CultureInfo.InvariantCulture);
            var cy = char.ToUpper(y[iy], CultureInfo.InvariantCulture);
            if (cx != cy)
                return cx.CompareTo(cy);

            ix++;
            iy++;
        }

        return (x.Length - ix).CompareTo(y.Length - iy);
    }

    private static int CompareNumbers(string x, ref int ix, string y, ref int iy)
    {
        var startX = ix;
        var startY = iy;
        while (ix < x.Length && char.IsDigit(x[ix]))
            ix++;
        while (iy < y.Length && char.IsDigit(y[iy]))
            iy++;

        var spanX = x.AsSpan(startX, ix - startX);
        var spanY = y.AsSpan(startY, iy - startY);

        var trimmedX = TrimLeadingZeros(spanX);
        var trimmedY = TrimLeadingZeros(spanY);

        if (trimmedX.Length != trimmedY.Length)
            return trimmedX.Length.CompareTo(trimmedY.Length);

        var cmp = trimmedX.CompareTo(trimmedY, StringComparison.Ordinal);
        if (cmp != 0)
            return cmp;

        // Mismo valor: el que tiene más ceros a la izquierda va después ("2" antes que "02").
        return spanX.Length.CompareTo(spanY.Length);
    }

    private static ReadOnlySpan<char> TrimLeadingZeros(ReadOnlySpan<char> digits)
    {
        var i = 0;
        while (i < digits.Length - 1 && digits[i] == '0')
            i++;
        return digits[i..];
    }
}
