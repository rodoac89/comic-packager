using System.IO.Compression;
using ComicPackager.Core.Models;

namespace ComicPackager.Core.Packing;

/// <summary>
/// CBZ = ZIP con extensión .cbz. Las páginas y ComicInfo.xml van en la raíz.
/// Imágenes con STORE (ya vienen comprimidas); XML con DEFLATE rápido.
/// </summary>
public sealed class ZipCbzPacker : IComicPacker
{
    public OutputFormat Format => OutputFormat.Cbz;

    public bool IsAvailable => true;

    public string? UnavailableReason => null;

    public Task PackAsync(
        string destinationPath,
        IReadOnlyList<PageItem> pages,
        string comicInfoXmlPath,
        IProgress<PackProgress>? progress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(pages);

        return Task.Run(() =>
        {
            var total = pages.Count + 1;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destinationPath))!);

            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            using var zip = ZipFile.Open(destinationPath, ZipArchiveMode.Create);

            for (var i = 0; i < pages.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = pages[i];
                var entryName = page.ArchiveEntryName(i + 1);
                if (entryName.Contains('/') || entryName.Contains('\\'))
                    throw new InvalidOperationException("Las páginas deben ir en la raíz del archivo, sin carpetas.");

                progress?.Report(new PackProgress
                {
                    Current = i + 1,
                    Total = total,
                    Message = $"Añadiendo {entryName}…",
                });

                zip.CreateEntryFromFile(page.SourcePath, entryName, CompressionLevel.NoCompression);
            }

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new PackProgress
            {
                Current = total,
                Total = total,
                Message = "Añadiendo ComicInfo.xml…",
            });

            zip.CreateEntryFromFile(comicInfoXmlPath, "ComicInfo.xml", CompressionLevel.Fastest);
        }, cancellationToken);
    }
}
