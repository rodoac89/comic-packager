using ComicPackager.Core.Import;
using ComicPackager.Core.Models;

namespace ComicPackager.Tests;

public class PageOrderTests
{
    [Fact]
    public void MoveSelectedUp_shifts_block_without_overtaking_selection()
    {
        var pages = List(1, 2, 3, 4);
        PageOrder.MoveSelectedUp(pages, [2, 3]);
        Assert.Equal([1, 3, 4, 2], pages);
    }

    [Fact]
    public void ReverseAll()
    {
        var pages = List(1, 2, 3, 4);
        PageOrder.ReverseAll(pages);
        Assert.Equal([4, 3, 2, 1], pages);
    }

    [Fact]
    public void ReverseSelection_only_touches_selected_slots()
    {
        var pages = List(1, 2, 3, 4, 5);
        PageOrder.ReverseSelection(pages, [1, 3, 4]);
        Assert.Equal([1, 5, 3, 4, 2], pages);
    }

    [Fact]
    public void MakeCover_moves_to_index_zero_and_sets_FrontCover()
    {
        var pages = new List<PageItem>
        {
            Page("a.jpg"),
            Page("b.jpg"),
            Page("c.jpg"),
        };
        pages[0].PageType = ComicPageType.FrontCover;

        PageOrder.MakeCover(pages, 2);

        Assert.Equal("c.jpg", pages[0].OriginalFileName);
        Assert.Equal(ComicPageType.FrontCover, pages[0].PageType);
        Assert.Equal(ComicPageType.Story, pages[1].PageType);
        Assert.Equal("a.jpg", pages[1].OriginalFileName);
    }

    [Fact]
    public void MoveTo_inserts_selection_as_a_block()
    {
        var pages = List(1, 2, 3, 4, 5);
        PageOrder.MoveTo(pages, [0, 2], 4);
        Assert.Equal([2, 4, 1, 3, 5], pages);
    }

    [Fact]
    public void ArchiveEntryName_uses_four_digit_padding_and_original_extension()
    {
        var page = new PageItem("/tmp/scan.PNG", "scan.PNG", ".PNG", 10);
        Assert.Equal("0001.png", page.ArchiveEntryName(1));
        Assert.Equal("0042.webp", new PageItem("/tmp/a.webp", "a.webp", ".webp", 1).ArchiveEntryName(42));
        Assert.Throws<ArgumentOutOfRangeException>(() => page.ArchiveEntryName(0));
    }

    private static List<int> List(params int[] values) => [.. values];

    private static PageItem Page(string name) =>
        new("/tmp/" + name, name, Path.GetExtension(name), 1);
}
