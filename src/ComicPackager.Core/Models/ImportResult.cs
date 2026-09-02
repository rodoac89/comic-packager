namespace ComicPackager.Core.Models;

public sealed class ImportResult
{
    public IReadOnlyList<PageItem> Pages { get; init; } = [];
    public IReadOnlyList<string> SkippedNonImages { get; init; } = [];
    public IReadOnlyList<CorruptFile> CorruptFiles { get; init; } = [];
    public IReadOnlyList<string> DuplicatesIgnored { get; init; } = [];
}

public sealed record CorruptFile(string Path, string Reason);
