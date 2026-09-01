namespace ComicPackager.Core.Import;

public static class SupportedImages
{
    public static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".gif",
        ".bmp",
        ".tif",
        ".tiff",
    };

    public static bool IsSupported(string pathOrExtension)
    {
        var ext = pathOrExtension.StartsWith('.')
            ? pathOrExtension
            : Path.GetExtension(pathOrExtension);
        return Extensions.Contains(ext);
    }

    /// <summary>
    /// Conserva la extensión original si es soportada. .jpeg se deja como .jpeg
    /// (no se normaliza a .jpg) para no alterar el archivo fuente.
    /// </summary>
    public static string CanonicalExtension(string path)
    {
        var ext = Path.GetExtension(path);
        return string.IsNullOrEmpty(ext) ? ".jpg" : ext.ToLowerInvariant();
    }
}
