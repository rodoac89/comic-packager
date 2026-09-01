using ComicPackager.Core.Import;
using SkiaSharp;

namespace ComicPackager.Tests;

public class ImageImporterTests
{
    [Fact]
    public void Imports_images_natural_sorted_and_skips_non_images()
    {
        var dir = CreateTempDir();
        try
        {
            WritePng(Path.Combine(dir, "page10.png"));
            WritePng(Path.Combine(dir, "page2.png"));
            File.WriteAllText(Path.Combine(dir, "notes.txt"), "nope");

            var result = new ImageImporter().ImportFolder(dir, recursive: false);

            Assert.Equal(2, result.Pages.Count);
            Assert.Equal("page2.png", result.Pages[0].OriginalFileName);
            Assert.Equal("page10.png", result.Pages[1].OriginalFileName);
            Assert.Contains(result.SkippedNonImages, p => p.EndsWith("notes.txt", StringComparison.OrdinalIgnoreCase));
            Assert.Empty(result.CorruptFiles);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Deduplicates_by_path_and_reports_corrupt()
    {
        var dir = CreateTempDir();
        try
        {
            var ok = Path.Combine(dir, "ok.jpg");
            WritePng(ok);
            var bad = Path.Combine(dir, "bad.jpg");
            File.WriteAllText(bad, "this is not an image");

            var importer = new ImageImporter();
            var first = importer.ImportFiles([ok, ok, bad]);

            Assert.Single(first.Pages);
            Assert.Single(first.DuplicatesIgnored);
            Assert.Single(first.CorruptFiles);
            Assert.Contains("bad.jpg", first.CorruptFiles[0].Path, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void Recurse_optional()
    {
        var dir = CreateTempDir();
        try
        {
            WritePng(Path.Combine(dir, "root.png"));
            var nested = Path.Combine(dir, "ch1");
            Directory.CreateDirectory(nested);
            WritePng(Path.Combine(nested, "nested.png"));

            var top = new ImageImporter().ImportFolder(dir, recursive: false);
            var all = new ImageImporter().ImportFolder(dir, recursive: true);

            Assert.Single(top.Pages);
            Assert.Equal(2, all.Pages.Count);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "panelpack-import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void WritePng(string path)
    {
        using var bitmap = new SKBitmap(2, 3);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }
}
