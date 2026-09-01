using System.IO.Compression;
using ComicPackager.Core.Metadata;
using ComicPackager.Core.Models;

namespace ComicPackager.Core.Packing;

public sealed class PackingService
{
    private readonly ZipCbzPacker _zipPacker;
    private readonly RarCbrPacker _rarPacker;
    private readonly ComicInfoGenerator _comicInfo;
    private readonly PackValidator _validator;

    public PackingService(
        ZipCbzPacker? zipPacker = null,
        RarCbrPacker? rarPacker = null,
        ComicInfoGenerator? comicInfo = null,
        PackValidator? validator = null)
    {
        _zipPacker = zipPacker ?? new ZipCbzPacker();
        _rarPacker = rarPacker ?? new RarCbrPacker();
        _comicInfo = comicInfo ?? new ComicInfoGenerator();
        _validator = validator ?? new PackValidator();
    }

    public bool IsCbrAvailable => _rarPacker.IsAvailable;

    public string CbrUnavailableReason => _rarPacker.UnavailableReason ?? RarBinaryDetector.UnavailableMessageEs;

    public ValidationResult Validate(PackRequest request) =>
        _validator.Validate(request, _rarPacker.IsAvailable);

    public string GetOutputPath(ComicMetadata metadata)
    {
        var fileName = OutputFileNameBuilder.EnsureExtension(metadata.OutputFileName, metadata.OutputFormat);
        fileName = OutputFileNameBuilder.SanitizeFileName(Path.GetFileNameWithoutExtension(fileName))
                   + metadata.OutputFormat.FileExtension();
        return Path.Combine(metadata.DestinationFolder, fileName);
    }

    public async Task<PackResult> PackAsync(
        PackRequest request,
        IProgress<PackProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (!validation.IsValid)
            throw new InvalidOperationException(validation.CombinedMessage);

        var pages = request.Pages.ToList();
        PageOrderEnsure(pages);

        var outputPath = GetOutputPath(request.Metadata);
        if (File.Exists(outputPath) && !request.OverwriteExisting)
        {
            throw new IOException($"El archivo ya existe: {outputPath}");
        }

        var tempDir = Path.Combine(Path.GetTempPath(), "ComicPackager", "pack-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var xmlPath = Path.Combine(tempDir, "ComicInfo.xml");

        try
        {
            progress?.Report(new PackProgress { Current = 0, Total = pages.Count + 2, Message = "Escribiendo ComicInfo.xml…" });
            _comicInfo.WriteToFile(xmlPath, request.Metadata, pages);

            var packer = request.Metadata.OutputFormat == OutputFormat.Cbr
                ? (IComicPacker)_rarPacker
                : _zipPacker;

            if (!packer.IsAvailable)
                throw new InvalidOperationException(packer.UnavailableReason);

            await packer.PackAsync(outputPath, pages, xmlPath, progress, cancellationToken).ConfigureAwait(false);

            VerifyOutput(outputPath, pages.Count, request.Metadata.OutputFormat);

            var size = new FileInfo(outputPath).Length;
            progress?.Report(new PackProgress
            {
                Current = 1,
                Total = 1,
                Message = "Empaquetado completado.",
            });

            return new PackResult
            {
                OutputPath = outputPath,
                FileSizeBytes = size,
                PageCount = pages.Count,
                Format = request.Metadata.OutputFormat,
            };
        }
        catch
        {
            TryDeleteFile(outputPath);
            throw;
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }
    }

    private static void PageOrderEnsure(IList<PageItem> pages)
    {
        Import.PageOrder.EnsureFrontCover(pages);
    }

    private static void VerifyOutput(string outputPath, int pageCount, OutputFormat format)
    {
        if (!File.Exists(outputPath))
            throw new InvalidOperationException("El archivo de salida no se creó.");

        var info = new FileInfo(outputPath);
        if (info.Length == 0)
            throw new InvalidOperationException("El archivo de salida está vacío.");

        if (format != OutputFormat.Cbz)
            return;

        using var zip = ZipFile.OpenRead(outputPath);
        if (zip.Entries.Count == 0)
            throw new InvalidOperationException("El CBZ no contiene entradas.");

        var names = zip.Entries.Select(e => e.FullName.Replace('\\', '/')).ToList();
        if (names.Any(n => n.Contains('/')))
            throw new InvalidOperationException("El CBZ contiene carpetas internas; las páginas deben ir en la raíz.");

        if (!names.Contains("ComicInfo.xml", StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Falta ComicInfo.xml en el CBZ.");

        if (names.Count < pageCount + 1)
            throw new InvalidOperationException("El CBZ no contiene todas las páginas esperadas.");
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort.
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
        catch
        {
            // Best-effort.
        }
    }
}
