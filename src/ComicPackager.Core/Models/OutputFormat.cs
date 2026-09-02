namespace ComicPackager.Core.Models;

public enum OutputFormat
{
    Cbz = 0,
    Cbr = 1,
}

public static class OutputFormatExtensions
{
    public static string FileExtension(this OutputFormat format) => format switch
    {
        OutputFormat.Cbr => ".cbr",
        _ => ".cbz",
    };
}
