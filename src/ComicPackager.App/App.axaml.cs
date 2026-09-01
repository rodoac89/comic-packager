using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ComicPackager.App.Services;
using ComicPackager.App.ViewModels;
using ComicPackager.App.Views;
using ComicPackager.Core.Import;
using ComicPackager.Core.Packing;
using ComicPackager.Core.Thumbnails;

namespace ComicPackager.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var settingsService = new SettingsService();
            var settings = settingsService.Load();
            RequestedThemeVariant = settings.Theme switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default,
            };

            var loc = new LocalizationService { Language = settings.Language };
            var dialogs = new DialogService
            {
                Loc = loc,
                LastSourceFolder = settings.LastSourceFolder,
            };
            var packing = new PackingService();
            var window = new MainWindow();
            dialogs.Owner = window;
            window.DataContext = new MainViewModel(
                loc,
                new ImageImporter(),
                packing,
                new ThumbnailCache(),
                settingsService,
                dialogs,
                settings);
            window.Closing += (_, _) =>
            {
                if (window.DataContext is MainViewModel vm)
                    vm.PersistSettings();
            };
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
