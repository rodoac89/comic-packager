using ComicPackager.Core.Models;

namespace ComicPackager.Core.Import;

public sealed class ImageImporter
{
    private static StringComparer PathComparer =>
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    /// <summary>
    /// Importa archivos sueltos. Ignora no-imágenes, avisa de corruptos y deduplica por ruta.
    /// El resultado queda ordenado con natural sort.
    /// </summary>
    public ImportResult ImportFiles(IEnumerable<string> paths, IEnumerable<string>? alreadyImportedPaths = null)
    {
        var existing = new HashSet<string>(alreadyImportedPaths ?? [], PathComparer);
        return ImportCore(ExpandExplicitFiles(paths), existing, sortByFileName: true);
    }

    /// <summary>
    /// Importa una carpeta. Recursivo opcional. Orden natural sobre la ruta relativa.
    /// </summary>
    public ImportResult ImportFolder(string folder, bool recursive, IEnumerable<string>? alreadyImportedPaths = null)
    {
        if (!Directory.Exists(folder))
        {
            return new ImportResult
            {
                CorruptFiles = [new CorruptFile(folder, "La carpeta no existe.")],
            };
        }

        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(folder, "*", option);
        }
        catch (Exception ex)
        {
            return new ImportResult
            {
                CorruptFiles = [new CorruptFile(folder, ex.Message)],
            };
        }

        var existing = new HashSet<string>(alreadyImportedPaths ?? [], PathComparer);
        return ImportCore(files, existing, sortByFileName: false);
    }

    private static IEnumerable<string> ExpandExplicitFiles(IEnumerable<string> paths)
    {
        foreach (var raw in paths)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            if (Directory.Exists(raw))
            {
                foreach (var file in Directory.EnumerateFiles(raw, "*", SearchOption.TopDirectoryOnly))
                    yield return file;
                continue;
            }

            yield return raw;
        }
    }

    private static ImportResult ImportCore(IEnumerable<string> files, HashSet<string> existing, bool sortByFileName)
    {
        var skipped = new List<string>();
        var corrupt = new List<CorruptFile>();
        var duplicates = new List<string>();
        var pages = new List<PageItem>();

        foreach (var file in files)
        {
            string full;
            try
            {
                full = Path.GetFullPath(file);
            }
            catch
            {
                skipped.Add(file);
                continue;
            }

            if (!SupportedImages.IsSupported(full))
            {
                skipped.Add(full);
                continue;
            }

            if (!existing.Add(full))
            {
                duplicates.Add(full);
                continue;
            }

            var page = ImageProbe.TryCreatePage(full, out var error);
            if (page is null)
            {
                corrupt.Add(new CorruptFile(full, error ?? "Archivo no decodificable."));
                existing.Remove(full);
                continue;
            }

            pages.Add(page);
        }

        pages.Sort((a, b) =>
        {
            var keyA = sortByFileName ? a.OriginalFileName : a.SourcePath;
            var keyB = sortByFileName ? b.OriginalFileName : b.SourcePath;
            return NaturalSortComparer.Instance.Compare(keyA, keyB);
        });

        return new ImportResult
        {
            Pages = pages,
            SkippedNonImages = skipped,
            CorruptFiles = corrupt,
            DuplicatesIgnored = duplicates,
        };
    }
}
