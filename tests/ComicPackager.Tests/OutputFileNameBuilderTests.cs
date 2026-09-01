using ComicPackager.Core.Metadata;
using ComicPackager.Core.Models;

namespace ComicPackager.Tests;

public class OutputFileNameBuilderTests
{
    [Fact]
    public void Series_volume_number()
    {
        var name = OutputFileNameBuilder.Build(new ComicMetadata
        {
            Series = "One Piece",
            Volume = 100,
            Number = "1052",
            OutputFormat = OutputFormat.Cbz,
        });
        Assert.Equal("One Piece v100 #1052.cbz", name);
    }

    [Fact]
    public void Falls_back_to_title_then_comic()
    {
        Assert.Equal("El título.cbz", OutputFileNameBuilder.Build(new ComicMetadata { Title = "El título" }));
        Assert.Equal("comic.cbz", OutputFileNameBuilder.Build(new ComicMetadata()));
    }

    [Fact]
    public void Cbr_extension_and_invalid_chars()
    {
        var name = OutputFileNameBuilder.Build(new ComicMetadata
        {
            Series = "Foo:Bar/Baz",
            Number = "1",
            OutputFormat = OutputFormat.Cbr,
        });
        Assert.Equal("Foo_Bar_Baz #1.cbr", name);
    }

    [Fact]
    public void EnsureExtension_replaces_previous_archive_extension()
    {
        Assert.Equal("tomo.cbr", OutputFileNameBuilder.EnsureExtension("tomo.cbz", OutputFormat.Cbr));
        Assert.Equal("tomo.cbz", OutputFileNameBuilder.EnsureExtension("tomo", OutputFormat.Cbz));
    }
}
