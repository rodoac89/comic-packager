using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ComicPackager.App.ViewModels;
using ComicPackager.App.Views;
using ComicPackager.Core.Import;
using ComicPackager.Core.Models;

namespace ComicPackager.App.Services;

public sealed class DialogService : IAppDialogs
{
    public Window? Owner { get; set; }

    public LocalizationService Loc { get; init; } = new();

    public string? LastSourceFolder { get; set; }

    public async Task<IReadOnlyList<string>> PickImageFilesAsync()
    {
        var top = Top();
        var start = await TryGetFolder(top, LastSourceFolder);
        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Loc["PickImages"],
            AllowMultiple = true,
            SuggestedStartLocation = start,
            FileTypeFilter =
            [
                new FilePickerFileType(Loc["Pages"])
                {
                    Patterns = SupportedImages.Extensions.Select(e => "*" + e).ToArray(),
                    MimeTypes = ["image/*"],
                    AppleUniformTypeIdentifiers = ["public.image"],
                },
            ],
        });

        return files
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Cast<string>()
            .ToList();
    }

    public async Task<string?> PickFolderAsync(string title, string? startPath = null)
    {
        var top = Top();
        var start = await TryGetFolder(top, startPath ?? LastSourceFolder);
        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            SuggestedStartLocation = start,
        });

        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    public Task AlertAsync(string title, string message) =>
        MessageWindow.ShowAsync(Owner, title, message, Loc["Ok"]);

    public async Task<bool> ConfirmAsync(string title, string message)
    {
        var result = await MessageWindow.ShowAsync(Owner, title, message, Loc["Yes"], Loc["Cancel"]);
        return result == MessageWindow.Primary;
    }

    public Task<bool> ConfirmOverwriteAsync(string path) =>
        ConfirmAsync(Loc["OverwriteTitle"], path + Environment.NewLine + Environment.NewLine + Loc["OverwriteMessage"]);

    public Task ShowImportWarningsAsync(ImportResult result)
    {
        var sb = new StringBuilder();
        if (result.CorruptFiles.Count > 0)
        {
            sb.AppendLine(Loc["Corrupt"] + ":");
            foreach (var file in result.CorruptFiles.Take(20))
                sb.AppendLine("  • " + file.Path + " — " + file.Reason);
            if (result.CorruptFiles.Count > 20)
                sb.AppendLine($"  … +{result.CorruptFiles.Count - 20}");
            sb.AppendLine();
        }

        if (result.SkippedNonImages.Count > 0)
        {
            sb.AppendLine(Loc["Skipped"] + $" ({result.SkippedNonImages.Count})");
            foreach (var file in result.SkippedNonImages.Take(8))
                sb.AppendLine("  • " + file);
            sb.AppendLine();
        }

        if (result.DuplicatesIgnored.Count > 0)
            sb.AppendLine(Loc["Duplicates"] + $": {result.DuplicatesIgnored.Count}");

        return AlertAsync(Loc["ImportWarnings"], sb.ToString().Trim());
    }

    public async Task ShowPackSuccessAsync(string path, string sizeLabel)
    {
        var body = string.Format(Loc["PackSuccessBody"], path, sizeLabel);
        var result = await MessageWindow.ShowAsync(
            Owner,
            Loc["PackSuccess"],
            body,
            Loc["OpenFolder"],
            Loc["Ok"]);
        if (result == MessageWindow.Primary)
            OpenInFileManager(path);
    }

    public async Task ShowLightboxAsync(IReadOnlyList<PageItemViewModel> pages, int index)
    {
        var window = new LightboxWindow(pages, index, Loc)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        if (Owner is not null)
            await window.ShowDialog(Owner);
        else
            window.Show();
    }

    public void OpenInFileManager(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "/select,\"" + path.Replace('/', '\\') + "\"",
                    UseShellExecute = true,
                });
                return;
            }

            var folder = File.Exists(path) ? Path.GetDirectoryName(path) : path;
            if (string.IsNullOrWhiteSpace(folder))
                return;

            if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", ["-R", path]);
                return;
            }

            Process.Start("xdg-open", [folder]);
        }
        catch
        {
            // El usuario puede abrir la carpeta a mano.
        }
    }

    private Window Top() =>
        Owner ?? throw new InvalidOperationException("La ventana principal aún no está lista.");

    private static async Task<IStorageFolder?> TryGetFolder(TopLevel top, string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return null;
        try
        {
            return await top.StorageProvider.TryGetFolderFromPathAsync(path);
        }
        catch
        {
            return null;
        }
    }
}
