namespace ComicPackager.Core.Models;

/// <summary>
/// Una página del cómic. El orden de la lista que las contiene es la
/// fuente de verdad del archivo final (0001, 0002, …).
/// </summary>
public sealed class PageItem
{
    public PageItem(
        string sourcePath,
        string originalFileName,
        string extension,
        long fileSizeBytes,
        int? pixelWidth = null,
        int? pixelHeight = null)
    {
        Id = Guid.NewGuid();
        SourcePath = sourcePath;
        OriginalFileName = originalFileName;
        Extension = NormalizeExtension(extension);
        FileSizeBytes = fileSizeBytes;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        PageType = ComicPageType.Story;
    }

    public Guid Id { get; }

    /// <summary>Ruta absoluta del archivo original. No se mueve ni se copia hasta empaquetar.</summary>
    public string SourcePath { get; }

    /// <summary>Nombre original para mostrar en la miniatura (p. ej. scan_02.jpg).</summary>
    public string OriginalFileName { get; }

    /// <summary>Extensión con punto, en minúsculas (.jpg). Se conserva en el archivo empaquetado.</summary>
    public string Extension { get; }

    public long FileSizeBytes { get; }

    public int? PixelWidth { get; }

    public int? PixelHeight { get; }

    public ComicPageType PageType { get; set; }

    public string ArchiveEntryName(int oneBasedIndex)
    {
        if (oneBasedIndex < 1 || oneBasedIndex > 9999)
            throw new ArgumentOutOfRangeException(nameof(oneBasedIndex), "El índice de página debe estar entre 1 y 9999.");

        return $"{oneBasedIndex:D4}{Extension}";
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return ".jpg";

        var ext = extension.StartsWith('.') ? extension : "." + extension;
        return ext.ToLowerInvariant();
    }
}
