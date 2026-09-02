using ComicPackager.Core.Models;

namespace ComicPackager.Core.Packing;

public interface IComicPacker
{
    OutputFormat Format { get; }

    bool IsAvailable { get; }

    string? UnavailableReason { get; }

    Task PackAsync(
        string destinationPath,
        IReadOnlyList<PageItem> pages,
        string comicInfoXmlPath,
        IProgress<PackProgress>? progress,
        CancellationToken cancellationToken);
}
