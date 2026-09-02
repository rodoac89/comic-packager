using System.Text;
using ComicPackager.Core.Models;

namespace ComicPackager.Core.Metadata;

public static class OutputFileNameBuilder
{
    public static string Build(ComicMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(metadata.Series))
            parts.Add(metadata.Series.Trim());
        else if (!string.IsNullOrWhiteSpace(metadata.Title))
            parts.Add(metadata.Title.Trim());

        if (metadata.Volume is int volume)
            parts.Add($"v{volume:00}");

        if (!string.IsNullOrWhiteSpace(metadata.Number))
            parts.Add($"#{metadata.Number.Trim()}");

        var stem = parts.Count > 0 ? string.Join(" ", parts) : "comic";
        stem = SanitizeFileName(stem);
        if (string.IsNullOrWhiteSpace(stem))
            stem = "comic";

        return stem + metadata.OutputFormat.FileExtension();
    }

    public static string EnsureExtension(string fileName, OutputFormat format)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "comic" + format.FileExtension();

        var trimmed = fileName.Trim();
        var ext = Path.GetExtension(trimmed);
        if (ext.Equals(".cbz", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".cbr", StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileNameWithoutExtension(trimmed) + format.FileExtension();
        }

        return trimmed + format.FileExtension();
    }

    public static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (var ch in name.Trim())
        {
            // También los ilegales en Windows, para que el nombre sea portable.
            if (Array.IndexOf(invalid, ch) >= 0 || ch is '<' or '>' or ':' or '"' or '/' or '\\' or '|' or '?' or '*')
                sb.Append('_');
            else
                sb.Append(ch);
        }

        var result = sb.ToString().Trim();
        while (result.EndsWith('.'))
            result = result[..^1].TrimEnd();

        return result;
    }
}
