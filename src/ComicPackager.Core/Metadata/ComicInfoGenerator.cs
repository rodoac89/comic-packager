using System.Text;
using System.Xml;
using ComicPackager.Core.Models;

namespace ComicPackager.Core.Metadata;

/// <summary>
/// Genera ComicInfo.xml compatible con Anansi / ComicInfo v2.0–v2.1.
/// UTF-8, campos vacíos omitidos, Pages en la raíz semántica del documento.
/// </summary>
public sealed class ComicInfoGenerator
{
    public const string CreatorNotes = "Created with Comic Packager";

    public string Generate(ComicMetadata metadata, IReadOnlyList<PageItem> pages)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(pages);

        using var stream = new MemoryStream();
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            IndentChars = "  ",
            OmitXmlDeclaration = false,
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace,
            Async = false,
        };

        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("ComicInfo");

            WriteIf(writer, "Title", metadata.Title);
            WriteIf(writer, "Series", metadata.Series);
            WriteIf(writer, "Number", metadata.Number);
            WriteIfInt(writer, "Volume", metadata.Volume);
            WriteIf(writer, "Summary", metadata.Summary);
            WriteIf(writer, "Notes", string.IsNullOrWhiteSpace(metadata.Notes) ? CreatorNotes : metadata.Notes.Trim());
            WriteIfInt(writer, "Year", metadata.Year);
            WriteIfInt(writer, "Month", metadata.Month);
            WriteIfInt(writer, "Day", metadata.Day);
            WriteIf(writer, "Writer", metadata.Writer);
            WriteIf(writer, "Penciller", metadata.Artist);
            WriteIf(writer, "Publisher", metadata.Publisher);
            WriteIf(writer, "Genre", metadata.Genre);
            writer.WriteElementString("PageCount", XmlConvert.ToString(pages.Count));
            WriteIf(writer, "LanguageISO", metadata.LanguageIso);
            WriteIf(writer, "Format", metadata.ResolveFormatElement());
            writer.WriteElementString("BlackAndWhite", metadata.BlackAndWhite ? "Yes" : "No");
            writer.WriteElementString("Manga", metadata.ResolveMangaElement());

            WritePages(writer, pages);

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public void WriteToFile(string path, ComicMetadata metadata, IReadOnlyList<PageItem> pages)
    {
        var xml = Generate(metadata, pages);
        File.WriteAllText(path, xml, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void WritePages(XmlWriter writer, IReadOnlyList<PageItem> pages)
    {
        writer.WriteStartElement("Pages");
        for (var i = 0; i < pages.Count; i++)
        {
            var page = pages[i];
            var type = page.PageType;
            if (i == 0 && type == ComicPageType.Story && pages.All(p => p.PageType != ComicPageType.FrontCover))
                type = ComicPageType.FrontCover;

            writer.WriteStartElement("Page");
            writer.WriteAttributeString("Image", XmlConvert.ToString(i));
            writer.WriteAttributeString("Type", type.ToString());
            writer.WriteAttributeString("ImageSize", XmlConvert.ToString(page.FileSizeBytes));
            if (page.PixelWidth is int w)
                writer.WriteAttributeString("ImageWidth", XmlConvert.ToString(w));
            if (page.PixelHeight is int h)
                writer.WriteAttributeString("ImageHeight", XmlConvert.ToString(h));
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    private static void WriteIf(XmlWriter writer, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;
        writer.WriteElementString(name, value.Trim());
    }

    private static void WriteIfInt(XmlWriter writer, string name, int? value)
    {
        if (value is null)
            return;
        writer.WriteElementString(name, XmlConvert.ToString(value.Value));
    }
}
