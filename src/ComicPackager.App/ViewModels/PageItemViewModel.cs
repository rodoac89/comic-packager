using Avalonia.Media.Imaging;
using ComicPackager.Core.Models;
using ComicPackager.Core.Thumbnails;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComicPackager.App.ViewModels;

public partial class PageItemViewModel : ViewModelBase, IDisposable
{
    public PageItemViewModel(PageItem model)
    {
        Model = model;
        PageType = model.PageType;
    }

    public PageItem Model { get; }

    [ObservableProperty]
    public partial Bitmap? Thumbnail { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IndexLabel))]
    [NotifyPropertyChangedFor(nameof(IsCover))]
    public partial int DisplayIndex { get; set; } = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCover))]
    [NotifyPropertyChangedFor(nameof(PageTypeLabel))]
    public partial ComicPageType PageType { get; set; }

    public string OriginalFileName => Model.OriginalFileName;

    public string IndexLabel => DisplayIndex.ToString("0000");

    public bool IsCover => PageType == ComicPageType.FrontCover || DisplayIndex == 1;

    public string PageTypeLabel => PageType.ToString();

    public string SourcePath => Model.SourcePath;

    partial void OnPageTypeChanged(ComicPageType value) => Model.PageType = value;

    [RelayCommand]
    private void SetPageType(string? typeName)
    {
        if (Enum.TryParse<ComicPageType>(typeName, out var type))
            PageType = type;
    }

    public async Task LoadThumbnailAsync(ThumbnailCache cache, CancellationToken cancellationToken)
    {
        var path = await cache.GetOrCreateAsync(Model.SourcePath, ThumbnailCache.DefaultMaxEdge, cancellationToken)
            .ConfigureAwait(false);
        if (path is null || cancellationToken.IsCancellationRequested)
            return;

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                Thumbnail?.Dispose();
                using var stream = File.OpenRead(path);
                Thumbnail = new Bitmap(stream);
            }
            catch
            {
                Thumbnail = null;
            }
        });
    }

    public void Dispose()
    {
        Thumbnail?.Dispose();
        Thumbnail = null;
        GC.SuppressFinalize(this);
    }
}
