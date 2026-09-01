using System.Xml.Linq;
using ComicPackager.Core.Metadata;

namespace ComicPackager.Tests;

public class ComicInfoExamplesTests
{
    [Fact]
    public void Manga_rtl_example_matches_generator()
    {
        var xml = new ComicInfoGenerator().Generate(
            ComicInfoGeneratorTests.MangaRtlMetadata(),
            SamplePages());
        AssertXmlEqual(ReadExample("ComicInfo.manga-rtl.xml"), xml);
    }

    [Fact]
    public void Comic_ltr_example_matches_generator()
    {
        var xml = new ComicInfoGenerator().Generate(
            ComicInfoGeneratorTests.WesternComicMetadata(),
            SamplePages());
        AssertXmlEqual(ReadExample("ComicInfo.comic-ltr.xml"), xml);
    }

    private static List<Core.Models.PageItem> SamplePages() =>
    [
        new(@"C:\comics\cover.jpg", "cover.jpg", ".jpg", 120_000, 1200, 1800)
        {
            PageType = Core.Models.ComicPageType.FrontCover,
        },
        new(@"C:\comics\p02.jpg", "p02.jpg", ".jpg", 98_000, 1200, 1800),
    ];

    private static string ReadExample(string name)
    {
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 8; i++)
        {
            var candidate = Path.Combine(dir, "examples", name);
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
            var parent = Directory.GetParent(dir);
            if (parent is null)
                break;
            dir = parent.FullName;
        }

        throw new FileNotFoundException("No se encontró " + name);
    }

    private static void AssertXmlEqual(string expected, string actual)
    {
        var a = XDocument.Parse(expected).ToString(SaveOptions.DisableFormatting);
        var b = XDocument.Parse(actual).ToString(SaveOptions.DisableFormatting);
        Assert.Equal(a, b);
    }
}
