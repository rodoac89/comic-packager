using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ComicPackager.App.Services;
using ComicPackager.App.ViewModels;
using ComicPackager.Core.Thumbnails;

namespace ComicPackager.App.Views;

public partial class LightboxWindow : Window
{
    private readonly IReadOnlyList<PageItemViewModel> _pages;
    private readonly LocalizationService _loc;
    private int _index;
    private Bitmap? _current;

    public LightboxWindow() : this([], 0, new LocalizationService())
    {
    }

    public LightboxWindow(IReadOnlyList<PageItemViewModel> pages, int index, LocalizationService loc)
    {
        _pages = pages;
        _loc = loc;
        _index = Math.Clamp(index, 0, Math.Max(0, pages.Count - 1));
        InitializeComponent();
        Title = loc["Lightbox"];
        Opened += (_, _) => ShowCurrent();
        Closed += (_, _) => DisposeCurrent();
    }

    private void OnPrev(object? sender, RoutedEventArgs e) => Move(-1);

    private void OnNext(object? sender, RoutedEventArgs e) => Move(1);

    private void OnClose(object? sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
            case Key.Up:
                Move(-1);
                e.Handled = true;
                break;
            case Key.Right:
            case Key.Down:
            case Key.Space:
                Move(1);
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }

    private void Move(int delta)
    {
        if (_pages.Count == 0)
            return;
        _index = (_index + delta + _pages.Count) % _pages.Count;
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        if (_pages.Count == 0)
            return;

        var page = _pages[_index];
        Caption.Text = $"{page.IndexLabel}  ·  {page.OriginalFileName}  ({_index + 1}/{_pages.Count})";

        var jpeg = ThumbnailCache.DecodeBoundedJpeg(page.SourcePath, maxEdge: 2048);
        DisposeCurrent();
        if (jpeg is null)
            return;

        using var stream = new MemoryStream(jpeg);
        _current = new Bitmap(stream);
        Preview.Source = _current;
    }

    private void DisposeCurrent()
    {
        Preview.Source = null;
        _current?.Dispose();
        _current = null;
    }
}
