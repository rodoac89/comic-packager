using System.Security.Cryptography;
using System.Text;
using SkiaSharp;

namespace ComicPackager.Core.Thumbnails;

/// <summary>
/// Miniaturas en disco temporal. Nunca carga el original a tamaño completo en la UI:
/// se decodifica ya escalado y se guarda un JPEG pequeño.
/// </summary>
public sealed class ThumbnailCache
{
    public const int DefaultMaxEdge = 256;

    private readonly string _root;

    public ThumbnailCache(string? root = null)
    {
        _root = root ?? Path.Combine(Path.GetTempPath(), "ComicPackager", "thumbs");
        Directory.CreateDirectory(_root);
    }

    public string Root => _root;

    public async Task<string?> GetOrCreateAsync(string sourcePath, int maxEdge, CancellationToken cancellationToken)
    {
        if (!File.Exists(sourcePath))
            return null;

        maxEdge = Math.Clamp(maxEdge, 32, 512);
        string key;
        try
        {
            key = CacheKey(sourcePath, maxEdge);
        }
        catch
        {
            return null;
        }
        var dest = Path.Combine(_root, key + ".jpg");
        if (File.Exists(dest))
            return dest;

        return await Task.Run(() => Create(sourcePath, dest, maxEdge), cancellationToken).ConfigureAwait(false);
    }

    public static string? Create(string sourcePath, string destPath, int maxEdge)
    {
        try
        {
            using var stream = File.OpenRead(sourcePath);
            using var codec = SKCodec.Create(stream);
            if (codec is null)
                return null;

            var info = codec.Info;
            var scale = Math.Min(1f, maxEdge / (float)Math.Max(info.Width, info.Height));
            var width = Math.Max(1, (int)Math.Round(info.Width * scale));
            var height = Math.Max(1, (int)Math.Round(info.Height * scale));
            var scaledInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

            using var bitmap = SKBitmap.Decode(codec, scaledInfo);
            if (bitmap is null)
                return null;

            Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 72);
            if (data is null)
                return null;

            using var output = File.Create(destPath);
            data.SaveTo(output);
            return destPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Decodifica el original ya limitado a maxEdge para la vista previa grande,
    /// sin volcar un escaneo de 6000px en memoria de la UI.
    /// </summary>
    public static byte[]? DecodeBoundedJpeg(string sourcePath, int maxEdge)
    {
        try
        {
            using var stream = File.OpenRead(sourcePath);
            using var codec = SKCodec.Create(stream);
            if (codec is null)
                return null;

            var info = codec.Info;
            var scale = Math.Min(1f, maxEdge / (float)Math.Max(info.Width, info.Height));
            var width = Math.Max(1, (int)Math.Round(info.Width * scale));
            var height = Math.Max(1, (int)Math.Round(info.Height * scale));
            var scaledInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var bitmap = SKBitmap.Decode(codec, scaledInfo);
            if (bitmap is null)
                return null;
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data?.ToArray();
        }
        catch
        {
            return null;
        }
    }

    public void Clear()
    {
        try
        {
            if (!Directory.Exists(_root))
                return;
            foreach (var file in Directory.EnumerateFiles(_root, "*.jpg"))
            {
                try { File.Delete(file); } catch { /* ignore */ }
            }
        }
        catch
        {
            // ignore
        }
    }

    private static string CacheKey(string sourcePath, int maxEdge)
    {
        var info = new FileInfo(sourcePath);
        var raw = $"{Path.GetFullPath(sourcePath)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}|{maxEdge}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash.AsSpan(0, 16)).ToLowerInvariant();
    }
}
