using ComicPackager.Core.Models;

namespace ComicPackager.Core.Import;

/// <summary>
/// Operaciones sobre la lista de páginas. La lista es la fuente de verdad del orden final.
/// </summary>
public static class PageOrder
{
    public static void MoveSelectedUp<T>(IList<T> pages, IReadOnlyList<int> selectedIndexes)
    {
        var selected = selectedIndexes.Where(i => i >= 0 && i < pages.Count).ToHashSet();
        for (var i = 0; i < pages.Count; i++)
        {
            if (!selected.Contains(i) || i == 0 || selected.Contains(i - 1))
                continue;
            Swap(pages, i, i - 1);
            selected.Remove(i);
            selected.Add(i - 1);
        }
    }

    public static void MoveSelectedDown<T>(IList<T> pages, IReadOnlyList<int> selectedIndexes)
    {
        var selected = selectedIndexes.Where(i => i >= 0 && i < pages.Count).ToHashSet();
        for (var i = pages.Count - 1; i >= 0; i--)
        {
            if (!selected.Contains(i) || i >= pages.Count - 1 || selected.Contains(i + 1))
                continue;
            Swap(pages, i, i + 1);
            selected.Remove(i);
            selected.Add(i + 1);
        }
    }

    public static void ReverseAll<T>(IList<T> pages)
    {
        var i = 0;
        var j = pages.Count - 1;
        while (i < j)
        {
            Swap(pages, i, j);
            i++;
            j--;
        }
    }

    public static void ReverseSelection<T>(IList<T> pages, IReadOnlyList<int> selectedIndexes)
    {
        var sorted = selectedIndexes.Distinct().Where(i => i >= 0 && i < pages.Count).OrderBy(i => i).ToList();
        if (sorted.Count < 2)
            return;

        var values = sorted.Select(i => pages[i]).ToList();
        values.Reverse();
        for (var n = 0; n < sorted.Count; n++)
            pages[sorted[n]] = values[n];
    }

    /// <summary>
    /// Mueve las páginas seleccionadas (en su orden relativo) para que empiecen en <paramref name="insertIndex"/>.
    /// </summary>
    public static void MoveTo<T>(IList<T> pages, IReadOnlyList<int> selectedIndexes, int insertIndex)
    {
        var sorted = selectedIndexes.Distinct().Where(i => i >= 0 && i < pages.Count).OrderBy(i => i).ToList();
        if (sorted.Count == 0)
            return;

        insertIndex = Math.Clamp(insertIndex, 0, pages.Count);
        var moving = sorted.Select(i => pages[i]).ToList();

        for (var n = sorted.Count - 1; n >= 0; n--)
        {
            pages.RemoveAt(sorted[n]);
            if (sorted[n] < insertIndex)
                insertIndex--;
        }

        insertIndex = Math.Clamp(insertIndex, 0, pages.Count);
        for (var n = 0; n < moving.Count; n++)
            pages.Insert(insertIndex + n, moving[n]);
    }

    public static void MakeCover(IList<PageItem> pages, int index)
    {
        if (index < 0 || index >= pages.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        foreach (var page in pages)
        {
            if (page.PageType == ComicPageType.FrontCover)
                page.PageType = ComicPageType.Story;
        }

        var cover = pages[index];
        cover.PageType = ComicPageType.FrontCover;
        if (index == 0)
            return;

        pages.RemoveAt(index);
        pages.Insert(0, cover);
    }

    public static void EnsureFrontCover(IList<PageItem> pages)
    {
        if (pages.Count == 0)
            return;
        if (pages.Any(p => p.PageType == ComicPageType.FrontCover))
            return;
        pages[0].PageType = ComicPageType.FrontCover;
    }

    private static void Swap<T>(IList<T> pages, int a, int b)
    {
        (pages[a], pages[b]) = (pages[b], pages[a]);
    }
}
