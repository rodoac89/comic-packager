using ComicPackager.Core.Models;
using ComicPackager.Core.Packing;

namespace ComicPackager.Tests;

public class PackValidatorTests
{
    private readonly PackValidator _validator = new();

    [Fact]
    public void Fails_without_pages_or_destination()
    {
        var result = _validator.Validate(new PackRequest
        {
            Pages = [],
            Metadata = new ComicMetadata(),
        }, rarAvailable: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Code == "NoPages");
        Assert.Contains(result.Issues, i => i.Code == "NoDestination");
    }

    [Fact]
    public void Cbr_without_rar_is_rejected()
    {
        var result = _validator.Validate(new PackRequest
        {
            Pages = [new PageItem("/tmp/a.jpg", "a.jpg", ".jpg", 1)],
            Metadata = new ComicMetadata
            {
                OutputFileName = "x.cbr",
                DestinationFolder = Path.GetTempPath(),
                OutputFormat = OutputFormat.Cbr,
            },
        }, rarAvailable: false);

        Assert.Contains(result.Issues, i => i.Code == "CbrUnavailable");
    }

    [Fact]
    public void Valid_cbz_request_passes()
    {
        var dest = Path.Combine(Path.GetTempPath(), "comicpackager-validator-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dest);
        try
        {
            var result = _validator.Validate(new PackRequest
            {
                Pages = [new PageItem("/tmp/a.jpg", "a.jpg", ".jpg", 1)],
                Metadata = new ComicMetadata
                {
                    OutputFileName = "demo.cbz",
                    DestinationFolder = dest,
                    OutputFormat = OutputFormat.Cbz,
                },
            }, rarAvailable: false);

            Assert.True(result.IsValid, result.CombinedMessage);
        }
        finally
        {
            Directory.Delete(dest, recursive: true);
        }
    }
}
