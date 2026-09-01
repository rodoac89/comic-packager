using ComicPackager.Core.Import;

namespace ComicPackager.Tests;

public class NaturalSortComparerTests
{
    private static IReadOnlyList<string> Sort(params string[] values) =>
        values.OrderBy(v => v, NaturalSortComparer.Instance).ToArray();

    [Fact]
    public void Page2_before_page10()
    {
        var sorted = Sort("pagina10.jpg", "pagina2.jpg", "pagina1.jpg");
        Assert.Equal(["pagina1.jpg", "pagina2.jpg", "pagina10.jpg"], sorted);
    }

    [Fact]
    public void Mixed_prefixes_and_padding()
    {
        var sorted = Sort("scan_10.png", "scan_2.png", "scan_02.png", "cover.jpg");
        Assert.Equal(["cover.jpg", "scan_2.png", "scan_02.png", "scan_10.png"], sorted);
    }

    [Fact]
    public void Case_insensitive_letters()
    {
        var sorted = Sort("B.jpg", "a.jpg", "C.jpg");
        Assert.Equal(["a.jpg", "B.jpg", "C.jpg"], sorted);
    }

    [Fact]
    public void Large_digit_runs_do_not_overflow()
    {
        var sorted = Sort(
            "x99999999999999999999.jpg",
            "x100000000000000000000.jpg");
        Assert.Equal(
            ["x99999999999999999999.jpg", "x100000000000000000000.jpg"],
            sorted);
    }

    [Fact]
    public void Nulls_and_empty()
    {
        var comparer = NaturalSortComparer.Instance;
        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare(null, "a") < 0);
        Assert.True(comparer.Compare("a", null) > 0);
        Assert.True(comparer.Compare("", "a") < 0);
    }

    [Fact]
    public void Comic_filenames_with_issue_numbers()
    {
        var sorted = Sort(
            "Amazing Spider-Man 010.jpg",
            "Amazing Spider-Man 2.jpg",
            "Amazing Spider-Man 009.jpg");
        Assert.Equal(
            [
                "Amazing Spider-Man 2.jpg",
                "Amazing Spider-Man 009.jpg",
                "Amazing Spider-Man 010.jpg",
            ],
            sorted);
    }
}
