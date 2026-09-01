using ComicPackager.App.ViewModels;
using ComicPackager.Core.Models;

namespace ComicPackager.App.Services;

internal sealed class NullDialogs : IAppDialogs
{
    public Task<IReadOnlyList<string>> PickImageFilesAsync() =>
        Task.FromResult<IReadOnlyList<string>>([]);

    public Task<string?> PickFolderAsync(string title, string? startPath = null) =>
        Task.FromResult<string?>(null);

    public Task AlertAsync(string title, string message) => Task.CompletedTask;

    public Task<bool> ConfirmAsync(string title, string message) => Task.FromResult(true);

    public Task<bool> ConfirmOverwriteAsync(string path) => Task.FromResult(true);

    public Task ShowImportWarningsAsync(ImportResult result) => Task.CompletedTask;

    public Task ShowPackSuccessAsync(string path, string sizeLabel) => Task.CompletedTask;

    public Task ShowLightboxAsync(IReadOnlyList<PageItemViewModel> pages, int index) => Task.CompletedTask;

    public void OpenInFileManager(string path)
    {
    }
}
