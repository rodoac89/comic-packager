namespace ComicPackager.App.Services;

public sealed class AppSettings
{
    public string Language { get; set; } = "es";
    public string Theme { get; set; } = "Dark";
    public string? LastDestinationFolder { get; set; }
    public string? LastSourceFolder { get; set; }
    public bool RecursiveFolders { get; set; }
    public double ThumbnailSize { get; set; } = 140;
}
