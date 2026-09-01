using ComicPackager.Core.Models;
using SkiaSharp;

namespace ComicPackager.Core.Import;

/// <summary>
/// Inspecciona una imagen sin decodificarla a tamaño completo.
/// </summary>
public static class ImageProbe
{
    public static bool TryProbe(string path, out int width, out int height, out string? error)
    {
        width = 0;
        height = 0;
        error = null;

        try
        {
            using var stream = File.OpenRead(path);
            using var codec = SKCodec.Create(stream, out var result);
            if (codec is null)
            {
                error = result == SKCodecResult.Success
                    ? "El archivo no se pudo decodificar."
                    : $"El archivo no se pudo decodificar ({result}).";
                return false;
            }

            width = codec.Info.Width;
            height = codec.Info.Height;
            if (width <= 0 || height <= 0)
            {
                error = "La imagen no tiene dimensiones válidas.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static PageItem? TryCreatePage(string path, out string? error)
    {
        error = null;
        var full = Path.GetFullPath(path);
        if (!File.Exists(full))
        {
            error = "El archivo no existe.";
            return null;
        }

        if (!TryProbe(full, out var width, out var height, out error))
            return null;

        var info = new FileInfo(full);
        return new PageItem(
            sourcePath: full,
            originalFileName: info.Name,
            extension: SupportedImages.CanonicalExtension(full),
            fileSizeBytes: info.Length,
            pixelWidth: width,
            pixelHeight: height);
    }
}
