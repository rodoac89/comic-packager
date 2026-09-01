using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Styling;
using ComicPackager.App.Services;
using ComicPackager.Core.Import;
using ComicPackager.Core.Metadata;
using ComicPackager.Core.Models;
using ComicPackager.Core.Packing;
using ComicPackager.Core.Thumbnails;
using ComicPackager.Core.Util;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComicPackager.App.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly ImageImporter _importer;
    private readonly PackingService _packing;
    private readonly ThumbnailCache _thumbnails;
    private readonly SettingsService _settingsService;
    private readonly IAppDialogs _dialogs;
    private readonly AppSettings _settings;
    private int _selectionAnchor = -1;
    private CancellationTokenSource? _thumbCts;
    private CancellationTokenSource? _packCts;

    public MainViewModel()
        : this(
            new LocalizationService(),
            new ImageImporter(),
            new PackingService(),
            new ThumbnailCache(),
            new SettingsService(),
            new NullDialogs(),
            new AppSettings())
    {
    }

    public MainViewModel(
        LocalizationService loc,
        ImageImporter importer,
        PackingService packing,
        ThumbnailCache thumbnails,
        SettingsService settingsService,
        IAppDialogs dialogs,
        AppSettings settings)
    {
        Loc = loc;
        _importer = importer;
        _packing = packing;
        _thumbnails = thumbnails;
        _settingsService = settingsService;
        _dialogs = dialogs;
        _settings = settings;

        Pages.CollectionChanged += OnPagesChanged;

        LanguageIso = loc.Language == "en" ? "en" : "es";
        RecursiveFolders = settings.RecursiveFolders;
        ThumbnailSize = Math.Clamp(settings.ThumbnailSize, 80, 240);
        DestinationFolder = settings.LastDestinationFolder
                            ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        ThemeName = settings.Theme;
        UiLanguage = settings.Language;
        IsCbrAvailable = packing.IsCbrAvailable;
        CbrUnavailableReason = packing.IsCbrAvailable
            ? string.Empty
            : loc.Language == "en"
                ? RarBinaryDetector.UnavailableMessageEn
                : RarBinaryDetector.UnavailableMessageEs;
        RefreshOutputName();
        RefreshStatus();
    }

    public LocalizationService Loc { get; }

    public ObservableCollection<PageItemViewModel> Pages { get; } = [];

    public IReadOnlyList<BookType> BookTypes { get; } =
        [BookType.Comic, BookType.Manga, BookType.ManhwaWebtoon];

    public IReadOnlyList<ComicPageType> PageTypes { get; } = Enum.GetValues<ComicPageType>();

    [ObservableProperty] public partial string Title { get; set; } = string.Empty;
    [ObservableProperty] public partial string Series { get; set; } = string.Empty;
    [ObservableProperty] public partial string Number { get; set; } = string.Empty;
    [ObservableProperty] public partial string VolumeText { get; set; } = string.Empty;
    [ObservableProperty] public partial string Writer { get; set; } = string.Empty;
    [ObservableProperty] public partial string Artist { get; set; } = string.Empty;
    [ObservableProperty] public partial string Publisher { get; set; } = string.Empty;
    [ObservableProperty] public partial string YearText { get; set; } = string.Empty;
    [ObservableProperty] public partial string MonthText { get; set; } = string.Empty;
    [ObservableProperty] public partial string DayText { get; set; } = string.Empty;
    [ObservableProperty] public partial string Genre { get; set; } = string.Empty;
    [ObservableProperty] public partial string LanguageIso { get; set; } = "es";
    [ObservableProperty] public partial string Summary { get; set; } = string.Empty;
    [ObservableProperty] public partial BookType BookType { get; set; } = BookType.Comic;
    [ObservableProperty] public partial bool RightToLeft { get; set; }
    [ObservableProperty] public partial bool BlackAndWhite { get; set; }
    [ObservableProperty] public partial OutputFormat OutputFormat { get; set; } = OutputFormat.Cbz;
    [ObservableProperty] public partial string OutputFileName { get; set; } = "comic.cbz";
    [ObservableProperty] public partial string DestinationFolder { get; set; } = string.Empty;
    [ObservableProperty] public partial bool RecursiveFolders { get; set; }
    [ObservableProperty] public partial double ThumbnailSize { get; set; } = 140;
    [ObservableProperty] public partial bool IsPacking { get; set; }
    [ObservableProperty] public partial double ProgressPercent { get; set; }
    [ObservableProperty] public partial string ProgressMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial string StatusText { get; set; } = string.Empty;
    [ObservableProperty] public partial string ReadingHint { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsCbrAvailable { get; set; }
    [ObservableProperty] public partial string CbrUnavailableReason { get; set; } = string.Empty;
    [ObservableProperty] public partial string ThemeName { get; set; } = "Dark";
    [ObservableProperty] public partial string UiLanguage { get; set; } = "es";
    [ObservableProperty] public partial bool HasPages { get; set; }

    public bool IsCbz
    {
        get => OutputFormat == OutputFormat.Cbz;
        set
        {
            if (value)
                OutputFormat = OutputFormat.Cbz;
        }
    }

    public bool IsCbr
    {
        get => OutputFormat == OutputFormat.Cbr;
        set
        {
            if (value && IsCbrAvailable)
                OutputFormat = OutputFormat.Cbr;
        }
    }

    public string BookTypeLabel(BookType type) => type switch
    {
        BookType.Manga => Loc["TypeManga"],
        BookType.ManhwaWebtoon => Loc["TypeManhwa"],
        _ => Loc["TypeComic"],
    };

    partial void OnSeriesChanged(string value) => RefreshOutputName();
    partial void OnTitleChanged(string value) => RefreshOutputName();
    partial void OnNumberChanged(string value) => RefreshOutputName();
    partial void OnVolumeTextChanged(string value) => RefreshOutputName();

    partial void OnOutputFormatChanged(OutputFormat value)
    {
        RefreshOutputName();
        OnPropertyChanged(nameof(IsCbz));
        OnPropertyChanged(nameof(IsCbr));
        PackCommand.NotifyCanExecuteChanged();
    }

    partial void OnDestinationFolderChanged(string value)
    {
        PackCommand.NotifyCanExecuteChanged();
        PersistSettings();
    }

    partial void OnOutputFileNameChanged(string value) => PackCommand.NotifyCanExecuteChanged();

    partial void OnThumbnailSizeChanged(double value) => PersistSettings();

    partial void OnRecursiveFoldersChanged(bool value) => PersistSettings();

    partial void OnThemeNameChanged(string value)
    {
        if (Application.Current is { } app)
        {
            app.RequestedThemeVariant = value switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };
        }
        OnPropertyChanged(nameof(ThemeIndex));
        PersistSettings();
    }

    partial void OnUiLanguageChanged(string value)
    {
        Loc.Language = value;
        _settings.Language = Loc.Language;
        OnPropertyChanged(nameof(Loc));
        OnPropertyChanged(string.Empty);
        ReadingHint = BookType switch
        {
            BookType.Manga => Loc["MangaHint"],
            BookType.ManhwaWebtoon => Loc["ManhwaHint"],
            _ => string.Empty,
        };
        if (!IsPacking)
            ProgressMessage = Loc["Ready"];
        CbrUnavailableReason = Loc.Language == "en"
            ? RarBinaryDetector.UnavailableMessageEn
            : RarBinaryDetector.UnavailableMessageEs;
        RefreshStatus();
        OnPropertyChanged(nameof(UiLanguageIndex));
        PersistSettings();
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var files = await _dialogs.PickImageFilesAsync();
        if (files.Count == 0)
            return;
        RememberSource(files[0]);
        await ImportPathsAsync(files, treatDirectoriesAsFolders: false);
    }

    [RelayCommand]
    private async Task AddFolderAsync()
    {
        var folder = await _dialogs.PickFolderAsync(Loc["PickFolder"]);
        if (string.IsNullOrWhiteSpace(folder))
            return;
        RememberSource(folder);
        var result = _importer.ImportFolder(folder, RecursiveFolders, ExistingPaths());
        await ApplyImportAsync(result);
    }

    [RelayCommand]
    private async Task BrowseDestinationAsync()
    {
        var folder = await _dialogs.PickFolderAsync(Loc["PickDestination"], DestinationFolder);
        if (!string.IsNullOrWhiteSpace(folder))
            DestinationFolder = folder;
    }

    public async Task ImportDroppedAsync(IReadOnlyList<string> paths)
    {
        if (paths.Count == 0)
            return;
        RememberSource(paths[0]);
        await ImportPathsAsync(paths, treatDirectoriesAsFolders: true);
    }

    [RelayCommand(CanExecute = nameof(CanModifyPages))]
    private void MoveUp()
    {
        var indexes = SelectedIndexes();
        var list = Pages.ToList();
        PageOrder.MoveSelectedUp(list, indexes);
        ReplacePages(list);
    }

    [RelayCommand(CanExecute = nameof(CanModifyPages))]
    private void MoveDown()
    {
        var indexes = SelectedIndexes();
        var list = Pages.ToList();
        PageOrder.MoveSelectedDown(list, indexes);
        ReplacePages(list);
    }

    [RelayCommand(CanExecute = nameof(CanModifyPages))]
    private void ReverseAll()
    {
        var list = Pages.ToList();
        PageOrder.ReverseAll(list);
        ReplacePages(list);
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private void ReverseSelected()
    {
        var list = Pages.ToList();
        PageOrder.ReverseSelection(list, SelectedIndexes());
        ReplacePages(list);
    }

    public int UiLanguageIndex
    {
        get => UiLanguage == "en" ? 1 : 0;
        set => UiLanguage = value == 1 ? "en" : "es";
    }

    public int ThemeIndex
    {
        get => ThemeName switch { "Light" => 1, "System" => 2, _ => 0 };
        set => ThemeName = value switch { 1 => "Light", 2 => "System", _ => "Dark" };
    }

    public int BookTypeIndex
    {
        get => (int)BookType;
        set => BookType = (BookType)value;
    }

    partial void OnBookTypeChanged(BookType value)
    {
        RightToLeft = value == BookType.Manga;
        ReadingHint = value switch
        {
            BookType.Manga => Loc["MangaHint"],
            BookType.ManhwaWebtoon => Loc["ManhwaHint"],
            _ => string.Empty,
        };
        OnPropertyChanged(nameof(BookTypeIndex));
    }

    [RelayCommand(CanExecute = nameof(CanModifyPages))]
    private void MakeCover(PageItemViewModel? page)
    {
        page ??= Pages.FirstOrDefault(p => p.IsSelected);
        if (page is null)
            return;
        var list = Pages.ToList();
        var index = list.IndexOf(page);
        if (index < 0)
            return;
        var models = list.Select(p => p.Model).ToList();
        PageOrder.MakeCover(models, index);
        var byId = list.ToDictionary(p => p.Model.Id);
        ReplacePages(models.Select(m => byId[m.Id]).ToList());
        foreach (var vm in Pages)
            vm.PageType = vm.Model.PageType;
    }

    [RelayCommand(CanExecute = nameof(CanModifySelection))]
    private async Task RemoveSelectedAsync()
    {
        var selected = Pages.Where(p => p.IsSelected).ToList();
        if (selected.Count == 0)
            return;
        if (selected.Count >= 5)
        {
            var ok = await _dialogs.ConfirmAsync(
                Loc["Confirm"],
                string.Format(Loc["DeleteMany"], selected.Count));
            if (!ok)
                return;
        }

        foreach (var page in selected)
        {
            Pages.Remove(page);
            page.Dispose();
        }
        RefreshIndexes();
    }

    [RelayCommand(CanExecute = nameof(CanModifyPages))]
    private async Task ClearAsync()
    {
        if (Pages.Count == 0)
            return;
        if (!await _dialogs.ConfirmAsync(Loc["Confirm"], Loc["ClearConfirm"]))
            return;
        ClearPages();
    }

    [RelayCommand]
    private async Task OpenLightboxAsync(PageItemViewModel? page)
    {
        page ??= Pages.FirstOrDefault(p => p.IsSelected) ?? Pages.FirstOrDefault();
        if (page is null)
            return;
        var index = Math.Max(0, Pages.IndexOf(page));
        await _dialogs.ShowLightboxAsync(Pages.ToList(), index);
    }

    [RelayCommand(CanExecute = nameof(CanPack))]
    private async Task PackAsync()
    {
        var metadata = BuildMetadata();
        var request = new PackRequest
        {
            Pages = Pages.Select(p => p.Model).ToList(),
            Metadata = metadata,
        };

        var validation = _packing.Validate(request);
        if (!validation.IsValid)
        {
            await _dialogs.AlertAsync(Loc["Error"], validation.CombinedMessage);
            return;
        }

        var outputPath = _packing.GetOutputPath(metadata);
        if (File.Exists(outputPath))
        {
            if (!await _dialogs.ConfirmOverwriteAsync(outputPath))
                return;
            request = new PackRequest
            {
                Pages = request.Pages,
                Metadata = metadata,
                OverwriteExisting = true,
            };
        }

        IsPacking = true;
        ProgressPercent = 0;
        ProgressMessage = Loc["Packing"];
        PackCommand.NotifyCanExecuteChanged();
        NotifyPageCommands();
        _packCts = new CancellationTokenSource();
        var progress = new Progress<PackProgress>(p =>
        {
            ProgressPercent = p.Percent;
            ProgressMessage = p.Message;
        });

        try
        {
            var result = await _packing.PackAsync(request, progress, _packCts.Token);
            await _dialogs.ShowPackSuccessAsync(result.OutputPath, ByteSizeFormatter.Format(result.FileSizeBytes));
        }
        catch (OperationCanceledException)
        {
            ProgressMessage = Loc["Ready"];
        }
        catch (Exception ex)
        {
            await _dialogs.AlertAsync(Loc["Error"], Humanize(ex));
        }
        finally
        {
            IsPacking = false;
            ProgressPercent = 0;
            ProgressMessage = Loc["Ready"];
            PackCommand.NotifyCanExecuteChanged();
            NotifyPageCommands();
        }
    }

    public void SelectPage(PageItemViewModel page, bool ctrl, bool shift)
    {
        var index = Pages.IndexOf(page);
        if (index < 0)
            return;

        if (shift && _selectionAnchor >= 0)
        {
            var from = Math.Min(_selectionAnchor, index);
            var to = Math.Max(_selectionAnchor, index);
            for (var i = 0; i < Pages.Count; i++)
                Pages[i].IsSelected = i >= from && i <= to;
        }
        else if (ctrl)
        {
            page.IsSelected = !page.IsSelected;
            _selectionAnchor = index;
        }
        else
        {
            foreach (var item in Pages)
                item.IsSelected = item == page;
            _selectionAnchor = index;
        }

        NotifyPageCommands();
    }

    public void DropPagesOnto(PageItemViewModel target)
    {
        var selectedIds = Pages.Where(p => p.IsSelected).Select(p => p.Model.Id).ToHashSet();
        if (selectedIds.Count == 0)
        {
            if (!target.IsSelected)
                selectedIds.Add(target.Model.Id);
            else
                return;
        }

        var selected = SelectedIndexes();
        if (selected.Count == 0)
            selected = [Pages.IndexOf(target)];
        var insert = Pages.IndexOf(target);
        if (insert < 0)
            return;
        var list = Pages.ToList();
        PageOrder.MoveTo(list, selected, insert);
        ReplacePages(list);
        foreach (var page in Pages)
            page.IsSelected = selectedIds.Contains(page.Model.Id);
    }

    public void PersistSettings()
    {
        _settings.Language = Loc.Language;
        _settings.Theme = ThemeName;
        _settings.LastDestinationFolder = DestinationFolder;
        _settings.RecursiveFolders = RecursiveFolders;
        _settings.ThumbnailSize = ThumbnailSize;
        _settingsService.Save(_settings);
    }

    public void Dispose()
    {
        _thumbCts?.Cancel();
        _packCts?.Cancel();
        ClearPages();
        GC.SuppressFinalize(this);
    }

    private bool CanPack() =>
        !IsPacking &&
        Pages.Count > 0 &&
        !string.IsNullOrWhiteSpace(OutputFileName) &&
        !string.IsNullOrWhiteSpace(DestinationFolder) &&
        (OutputFormat == OutputFormat.Cbz || IsCbrAvailable);

    private bool CanModifyPages() => !IsPacking && Pages.Count > 0;

    private bool CanModifySelection() => !IsPacking && Pages.Any(p => p.IsSelected);

    private async Task ImportPathsAsync(IReadOnlyList<string> paths, bool treatDirectoriesAsFolders)
    {
        var files = new List<string>();
        var folders = new List<string>();
        foreach (var path in paths)
        {
            if (Directory.Exists(path) && treatDirectoriesAsFolders)
                folders.Add(path);
            else
                files.Add(path);
        }

        var combined = new ImportResult();
        var pages = new List<PageItem>();
        var skipped = new List<string>();
        var corrupt = new List<CorruptFile>();
        var duplicates = new List<string>();

        if (files.Count > 0)
        {
            var r = _importer.ImportFiles(files, ExistingPaths());
            pages.AddRange(r.Pages);
            skipped.AddRange(r.SkippedNonImages);
            corrupt.AddRange(r.CorruptFiles);
            duplicates.AddRange(r.DuplicatesIgnored);
        }

        foreach (var folder in folders)
        {
            var r = _importer.ImportFolder(folder, RecursiveFolders, ExistingPaths().Concat(pages.Select(p => p.SourcePath)));
            pages.AddRange(r.Pages);
            skipped.AddRange(r.SkippedNonImages);
            corrupt.AddRange(r.CorruptFiles);
            duplicates.AddRange(r.DuplicatesIgnored);
        }

        combined = new ImportResult
        {
            Pages = pages,
            SkippedNonImages = skipped,
            CorruptFiles = corrupt,
            DuplicatesIgnored = duplicates,
        };
        await ApplyImportAsync(combined);
    }

    private async Task ApplyImportAsync(ImportResult result)
    {
        foreach (var page in result.Pages)
            Pages.Add(new PageItemViewModel(page));

        RefreshIndexes();
        await LoadNewThumbnailsAsync();

        if (result.CorruptFiles.Count > 0 ||
            (result.Pages.Count == 0 && result.SkippedNonImages.Count > 0))
        {
            await _dialogs.ShowImportWarningsAsync(result);
        }
    }

    private async Task LoadNewThumbnailsAsync()
    {
        _thumbCts?.Cancel();
        _thumbCts = new CancellationTokenSource();
        var token = _thumbCts.Token;
        var pending = Pages.Where(p => p.Thumbnail is null).ToList();
        foreach (var page in pending)
        {
            if (token.IsCancellationRequested)
                break;
            try
            {
                await page.LoadThumbnailAsync(_thumbnails, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void ReplacePages(List<PageItemViewModel> list)
    {
        Pages.CollectionChanged -= OnPagesChanged;
        Pages.Clear();
        foreach (var page in list)
            Pages.Add(page);
        Pages.CollectionChanged += OnPagesChanged;
        RefreshIndexes();
        NotifyPageCommands();
        PackCommand.NotifyCanExecuteChanged();
        RefreshStatus();
        HasPages = Pages.Count > 0;
    }

    private void ClearPages()
    {
        foreach (var page in Pages)
            page.Dispose();
        Pages.Clear();
        _selectionAnchor = -1;
    }

    private void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasPages = Pages.Count > 0;
        RefreshIndexes();
        RefreshStatus();
        PackCommand.NotifyCanExecuteChanged();
        NotifyPageCommands();
    }

    private void RefreshIndexes()
    {
        PageOrder.EnsureFrontCover(Pages.Select(p => p.Model).ToList());
        for (var i = 0; i < Pages.Count; i++)
        {
            Pages[i].DisplayIndex = i + 1;
            Pages[i].PageType = Pages[i].Model.PageType;
        }
    }

    private void RefreshStatus()
    {
        var bytes = Pages.Sum(p => p.Model.FileSizeBytes);
        StatusText = string.Format(Loc["PagesStatus"], Pages.Count, ByteSizeFormatter.Format(bytes));
        if (string.IsNullOrEmpty(ProgressMessage))
            ProgressMessage = Loc["Ready"];
    }

    private void RefreshOutputName()
    {
        OutputFileName = OutputFileNameBuilder.Build(BuildMetadata());
    }

    private ComicMetadata BuildMetadata()
    {
        return new ComicMetadata
        {
            Title = EmptyToNull(Title),
            Series = EmptyToNull(Series),
            Number = EmptyToNull(Number),
            Volume = ParseInt(VolumeText),
            Writer = EmptyToNull(Writer),
            Artist = EmptyToNull(Artist),
            Publisher = EmptyToNull(Publisher),
            Year = ParseInt(YearText),
            Month = ParseInt(MonthText),
            Day = ParseInt(DayText),
            Genre = EmptyToNull(Genre),
            LanguageIso = EmptyToNull(LanguageIso),
            Summary = EmptyToNull(Summary),
            BookType = BookType,
            RightToLeft = RightToLeft,
            BlackAndWhite = BlackAndWhite,
            OutputFormat = OutputFormat,
            OutputFileName = OutputFileName,
            DestinationFolder = DestinationFolder,
        };
    }

    private IEnumerable<string> ExistingPaths() => Pages.Select(p => p.Model.SourcePath);

    private List<int> SelectedIndexes() =>
        Pages.Select((p, i) => (p, i)).Where(x => x.p.IsSelected).Select(x => x.i).ToList();

    private void NotifyPageCommands()
    {
        MoveUpCommand.NotifyCanExecuteChanged();
        MoveDownCommand.NotifyCanExecuteChanged();
        ReverseAllCommand.NotifyCanExecuteChanged();
        ReverseSelectedCommand.NotifyCanExecuteChanged();
        MakeCoverCommand.NotifyCanExecuteChanged();
        RemoveSelectedCommand.NotifyCanExecuteChanged();
        ClearCommand.NotifyCanExecuteChanged();
    }

    private void RememberSource(string path)
    {
        try
        {
            _settings.LastSourceFolder = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
            PersistSettings();
        }
        catch
        {
            // ignore
        }
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParseInt(string? text) =>
        int.TryParse(text, out var n) ? n : null;

    private static string Humanize(Exception ex) =>
        ex.InnerException is null ? ex.Message : $"{ex.Message}{Environment.NewLine}{ex.InnerException.Message}";
}
