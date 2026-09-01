using System.Xml.Linq;
using ComicPackager.Core.Metadata;
using ComicPackager.Core.Models;

namespace ComicPackager.Tests;

public class ComicInfoGeneratorTests
{
    private readonly ComicInfoGenerator _generator = new();

    [Fact]
    public void Manga_rtl_writes_YesAndRightToLeft()
    {
        var xml = _generator.Generate(MangaRtlMetadata(), SamplePages());
        var doc = XDocument.Parse(xml);

        Assert.Equal("utf-8", doc.Declaration?.Encoding, ignoreCase: true);
        Assert.Equal("One Piece", Element(doc, "Title"));
        Assert.Equal("One Piece", Element(doc, "Series"));
        Assert.Equal("1052", Element(doc, "Number"));
        Assert.Equal("100", Element(doc, "Volume"));
        Assert.Equal("Eiichiro Oda", Element(doc, "Writer"));
        Assert.Equal("Eiichiro Oda", Element(doc, "Penciller"));
        Assert.Equal("Shueisha", Element(doc, "Publisher"));
        Assert.Equal("2022", Element(doc, "Year"));
        Assert.Equal("5", Element(doc, "Month"));
        Assert.Equal("2", Element(doc, "Day"));
        Assert.Equal("Shonen", Element(doc, "Genre"));
        Assert.Equal("ja", Element(doc, "LanguageISO"));
        Assert.Equal("Digital", Element(doc, "Format"));
        Assert.Equal("Yes", Element(doc, "BlackAndWhite"));
        Assert.Equal("YesAndRightToLeft", Element(doc, "Manga"));
        Assert.Equal("2", Element(doc, "PageCount"));
        Assert.Equal("Created with Comic Packager", Element(doc, "Notes"));
        Assert.Contains("Sombrero de Paja", Element(doc, "Summary"));

        var pages = doc.Root!.Element("Pages")!.Elements("Page").ToList();
        Assert.Equal(2, pages.Count);
        Assert.Equal("0", pages[0].Attribute("Image")!.Value);
        Assert.Equal("FrontCover", pages[0].Attribute("Type")!.Value);
        Assert.Equal("1", pages[1].Attribute("Image")!.Value);
        Assert.Equal("Story", pages[1].Attribute("Type")!.Value);
        Assert.Equal("1200", pages[0].Attribute("ImageWidth")!.Value);
        Assert.Equal("1800", pages[0].Attribute("ImageHeight")!.Value);
    }

    [Fact]
    public void Manga_without_rtl_writes_Yes()
    {
        var metadata = MangaRtlMetadata();
        metadata.RightToLeft = false;
        var xml = _generator.Generate(metadata, SamplePages());
        var doc = XDocument.Parse(xml);
        Assert.Equal("Yes", Element(doc, "Manga"));
        Assert.NotEqual("YesAndRightToLeft", Element(doc, "Manga"));
    }

    [Fact]
    public void Western_comic_writes_Manga_No()
    {
        var xml = _generator.Generate(WesternComicMetadata(), SamplePages());
        var doc = XDocument.Parse(xml);

        Assert.Equal("No", Element(doc, "Manga"));
        Assert.Equal("No", Element(doc, "BlackAndWhite"));
        Assert.Equal("Digital", Element(doc, "Format"));
        Assert.Equal("Batman", Element(doc, "Series"));
        Assert.Equal("Scott Snyder", Element(doc, "Writer"));
        Assert.Equal("Greg Capullo", Element(doc, "Penciller"));
        Assert.Equal("DC Comics", Element(doc, "Publisher"));
        Assert.Equal("en", Element(doc, "LanguageISO"));
    }

    [Fact]
    public void Manhwa_is_ltr_web_and_manga_no()
    {
        var metadata = new ComicMetadata
        {
            Title = "Solo Leveling",
            Series = "Solo Leveling",
            Number = "1",
            BookType = BookType.ManhwaWebtoon,
            RightToLeft = false,
            LanguageIso = "ko",
        };
        var xml = _generator.Generate(metadata, SamplePages());
        var doc = XDocument.Parse(xml);
        Assert.Equal("No", Element(doc, "Manga"));
        Assert.Equal("Web", Element(doc, "Format"));
    }

    [Fact]
    public void Empty_optional_fields_are_omitted()
    {
        var metadata = new ComicMetadata { BookType = BookType.Comic };
        var xml = _generator.Generate(metadata, SamplePages());
        var doc = XDocument.Parse(xml);

        Assert.Null(doc.Root!.Element("Title"));
        Assert.Null(doc.Root.Element("Series"));
        Assert.Null(doc.Root.Element("Writer"));
        Assert.Null(doc.Root.Element("Year"));
        Assert.Equal("Created with Comic Packager", Element(doc, "Notes"));
        Assert.Equal("No", Element(doc, "Manga"));
        Assert.Equal("2", Element(doc, "PageCount"));
    }

    [Fact]
    public void Xml_is_utf8_without_bom_and_escapes_special_characters()
    {
        var metadata = new ComicMetadata
        {
            Title = "Tom & Jerry <Special>",
            Summary = "Años 90 — \"quote\"",
            BookType = BookType.Comic,
        };
        var xml = _generator.Generate(metadata, SamplePages());
        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", xml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain('\uFEFF', xml);
        var doc = XDocument.Parse(xml);
        Assert.Equal("Tom & Jerry <Special>", Element(doc, "Title"));
        Assert.Equal("Años 90 — \"quote\"", Element(doc, "Summary"));
    }

    [Fact]
    public void First_page_becomes_FrontCover_if_none_marked()
    {
        var pages = SamplePages();
        pages[0].PageType = ComicPageType.Story;
        var xml = _generator.Generate(new ComicMetadata(), pages);
        var cover = XDocument.Parse(xml).Root!.Element("Pages")!.Elements("Page").First();
        Assert.Equal("FrontCover", cover.Attribute("Type")!.Value);
    }

    [Fact]
    public void Respects_explicit_non_cover_type_on_first_page_if_another_cover_exists()
    {
        var pages = SamplePages();
        pages[0].PageType = ComicPageType.Advertisement;
        pages[1].PageType = ComicPageType.FrontCover;
        var xml = _generator.Generate(new ComicMetadata(), pages);
        var parsed = XDocument.Parse(xml).Root!.Element("Pages")!.Elements("Page").ToList();
        Assert.Equal("Advertisement", parsed[0].Attribute("Type")!.Value);
        Assert.Equal("FrontCover", parsed[1].Attribute("Type")!.Value);
    }

    internal static ComicMetadata MangaRtlMetadata() => new()
    {
        Title = "One Piece",
        Series = "One Piece",
        Number = "1052",
        Volume = 100,
        Writer = "Eiichiro Oda",
        Artist = "Eiichiro Oda",
        Publisher = "Shueisha",
        Year = 2022,
        Month = 5,
        Day = 2,
        Genre = "Shonen",
        LanguageIso = "ja",
        Summary = "Luffy y los Sombrero de Paja continúan su viaje pirata.",
        BookType = BookType.Manga,
        RightToLeft = true,
        BlackAndWhite = true,
    };

    internal static ComicMetadata WesternComicMetadata() => new()
    {
        Title = "The Court of Owls",
        Series = "Batman",
        Number = "1",
        Volume = 2,
        Writer = "Scott Snyder",
        Artist = "Greg Capullo",
        Publisher = "DC Comics",
        Year = 2011,
        Month = 11,
        Day = 1,
        Genre = "Superhero",
        LanguageIso = "en",
        Summary = "Batman enfrenta a la Corte de los Búhos.",
        BookType = BookType.Comic,
        RightToLeft = false,
        BlackAndWhite = false,
    };

    private static List<PageItem> SamplePages() =>
    [
        new PageItem(@"C:\comics\cover.jpg", "cover.jpg", ".jpg", 120_000, 1200, 1800)
        {
            PageType = ComicPageType.FrontCover,
        },
        new PageItem(@"C:\comics\p02.jpg", "p02.jpg", ".jpg", 98_000, 1200, 1800),
    ];

    private static string Element(XDocument doc, string name) =>
        doc.Root?.Element(name)?.Value ?? string.Empty;
}
