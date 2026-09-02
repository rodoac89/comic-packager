using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ComicPackager.App.Views;

public partial class MessageWindow : Window
{
    public const string Primary = "primary";
    public const string Secondary = "secondary";

    private string _result = Secondary;

    public MessageWindow()
    {
        InitializeComponent();
    }

    public static async Task<string> ShowAsync(
        Window? owner,
        string title,
        string message,
        string primary,
        string? secondary = null)
    {
        var window = new MessageWindow
        {
            Title = title,
        };
        window.MessageText.Text = message;
        window.PrimaryButton.Content = primary;
        if (!string.IsNullOrWhiteSpace(secondary))
        {
            window.SecondaryButton.Content = secondary;
            window.SecondaryButton.IsVisible = true;
            window.SecondaryButton.IsCancel = true;
        }
        else
        {
            window.PrimaryButton.IsCancel = true;
        }

        if (owner is not null)
            await window.ShowDialog(owner);
        else
        {
            window.Show();
            await window.WaitClosedAsync();
        }

        return window._result;
    }

    private void OnPrimary(object? sender, RoutedEventArgs e)
    {
        _result = Primary;
        Close();
    }

    private void OnSecondary(object? sender, RoutedEventArgs e)
    {
        _result = Secondary;
        Close();
    }

    private Task WaitClosedAsync()
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Closed += (_, _) => tcs.TrySetResult(null);
        return tcs.Task;
    }
}
