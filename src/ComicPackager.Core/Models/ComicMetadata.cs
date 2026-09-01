namespace ComicPackager.Core.Models;

public sealed class ComicMetadata
{
    public string? Title { get; set; }
    public string? Series { get; set; }
    public string? Number { get; set; }
    public int? Volume { get; set; }
    public string? Writer { get; set; }
    public string? Artist { get; set; }
    public string? Publisher { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Day { get; set; }
    public string? Genre { get; set; }
    public string? LanguageIso { get; set; }
    public string? Summary { get; set; }
    public BookType BookType { get; set; } = BookType.Comic;
    public bool RightToLeft { get; set; }
    public bool BlackAndWhite { get; set; }
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Cbz;
    public string OutputFileName { get; set; } = "comic.cbz";
    public string DestinationFolder { get; set; } = string.Empty;
    public string Notes { get; set; } = "Created with PanelPack";

    /// <summary>
    /// Valor del elemento Manga según tipo y checkbox RTL.
    /// Marcar RTL no reordena páginas: solo escribe metadatos.
    /// </summary>
    public string ResolveMangaElement()
    {
        return BookType switch
        {
            BookType.Manga when RightToLeft => "YesAndRightToLeft",
            BookType.Manga => "Yes",
            _ => "No",
        };
    }

    /// <summary>Format de ComicInfo: Web para manhwa/webtoon, Digital para el resto.</summary>
    public string ResolveFormatElement()
    {
        return BookType == BookType.ManhwaWebtoon ? "Web" : "Digital";
    }
}
