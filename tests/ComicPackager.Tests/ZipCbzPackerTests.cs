using System.IO.Compression;
using ComicPackager.Core.Metadata;
using ComicPackager.Core.Models;
using ComicPackager.Core.Packing;
using SkiaSharp;

namespace ComicPackager.Tests;

public class ZipCbzPackerTests
{
    [Fact]
    public async Task Packs_pages_at_zip_root_with_padded_names_and_comicinfo()
    {
        var dir = Path.Combine(Path.GetTempPath(), "comicpackager-zip-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var img1 = Path.Combine(dir, "origB.png");
            var img2 = Path.Combine(dir, "origA.png");
            WritePng(img1);
            WritePng(img2);

            var pages = new List<PageItem>
            {
                new(img1, "origB.png", ".png", new FileInfo(img1).Length, 2, 3) { PageType = ComicPageType.FrontCover },
                new(img2, "origA.png", ".png", new FileInfo(img2).Length, 2, 3),
            };

            var metadata = new ComicMetadata
            {
                Title = "Demo",
                Series = "Demo",
                Number = "1",
                BookType = BookType.Comic,
                OutputFormat = OutputFormat.Cbz,
                OutputFileName = "Demo #1.cbz",
                DestinationFolder = dir,
            };

            var result = await new PackingService().PackAsync(new PackRequest
            {
                Pages = pages,
                Metadata = metadata,
                OverwriteExisting = true,
            });

            Assert.True(File.Exists(result.OutputPath));
            Assert.True(result.FileSizeBytes > 0);

            using var zip = ZipFile.OpenRead(result.OutputPath);
            var names = zip.Entries.Select(e => e.FullName.Replace('\\', '/')).OrderBy(n => n).ToList();
            Assert.Equal(["0001.png", "0002.png", "ComicInfo.xml"], names);

            var xml = new StreamReader(zip.GetEntry("ComicInfo.xml")!.Open()).ReadToEnd();
            Assert.Contains("<Manga>No</Manga>", xml);
            Assert.Contains("<Title>Demo</Title>", xml);
            Assert.Contains("Created with Comic Packager", xml);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static void WritePng(string path)
    {
        using var bitmap = new SKBitmap(2, 3);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Blue);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }
}
