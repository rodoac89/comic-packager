using ComicPackager.App.ViewModels;
using ComicPackager.Core.Models;

namespace ComicPackager.App.Services;

public interface IAppDialogs
{
    Task<IReadOnlyList<string>> PickImageFilesAsync();
    Task<string?> PickFolderAsync(string title, string? startPath = null);
    Task AlertAsync(string title, string message);
    Task<bool> ConfirmAsync(string title, string message);
    Task<bool> ConfirmOverwriteAsync(string path);
    Task ShowImportWarningsAsync(ImportResult result);
    Task ShowPackSuccessAsync(string path, string sizeLabel);
    Task ShowLightboxAsync(IReadOnlyList<PageItemViewModel> pages, int index);
    void OpenInFileManager(string path);
}
