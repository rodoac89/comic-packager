namespace ComicPackager.Core.Models;

public sealed class PackRequest
{
    public required IReadOnlyList<PageItem> Pages { get; init; }
    public required ComicMetadata Metadata { get; init; }
    public bool OverwriteExisting { get; init; }
}

public sealed class PackProgress
{
    public int Current { get; init; }
    public int Total { get; init; }
    public string Message { get; init; } = string.Empty;
    public double Percent => Total <= 0 ? 0 : (100.0 * Current / Total);
}

public sealed class PackResult
{
    public required string OutputPath { get; init; }
    public required long FileSizeBytes { get; init; }
    public required int PageCount { get; init; }
    public required OutputFormat Format { get; init; }
}
