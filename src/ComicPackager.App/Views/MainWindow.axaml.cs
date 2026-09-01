using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ComicPackager.App.ViewModels;

namespace ComicPackager.App.Views;

public partial class MainWindow : Window
{
    private PageItemViewModel? _dragPage;
    private Point _dragStart;
    private bool _dragging;

    public MainWindow()
    {
        InitializeComponent();
        DragDrop.AddDragOverHandler(this, OnDragOver);
        DragDrop.AddDropHandler(this, OnDrop);
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (e.Key == Key.Delete && FocusManager?.GetFocusedElement() is not TextBox)
        {
            if (vm.RemoveSelectedCommand.CanExecute(null))
                vm.RemoveSelectedCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        var hasFiles = e.DataTransfer is { } transfer && transfer.Contains(DataFormat.File);
        e.DragEffects = hasFiles ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        var files = e.DataTransfer?.TryGetFiles();
        if (files is null)
            return;

        var paths = files
            .Select(f => f.TryGetLocalPath())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Cast<string>()
            .ToList();
        await vm.ImportDroppedAsync(paths);
        e.Handled = true;
    }

    private void OnCardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control || control.DataContext is not PageItemViewModel page)
            return;
        if (DataContext is not MainViewModel vm)
            return;

        var mods = e.KeyModifiers;
        vm.SelectPage(page, mods.HasFlag(KeyModifiers.Control) || mods.HasFlag(KeyModifiers.Meta), mods.HasFlag(KeyModifiers.Shift));

        _dragPage = page;
        _dragStart = e.GetPosition(this);
        _dragging = false;
        e.Pointer.Capture(control);
    }

    private void OnCardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_dragPage is null)
            return;
        var pos = e.GetPosition(this);
        if (!_dragging && Distance(pos, _dragStart) > 8)
            _dragging = true;
    }

    private void OnCardPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (_dragging && _dragPage is not null && DataContext is MainViewModel vm)
            {
                var hit = this.InputHitTest(e.GetPosition(this));
                var target = FindPage(hit);
                if (target is not null && target != _dragPage)
                    vm.DropPagesOnto(target);
            }
        }
        finally
        {
            _dragPage = null;
            _dragging = false;
            e.Pointer.Capture(null);
        }
    }

    private async void OnCardDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { DataContext: PageItemViewModel page } &&
            DataContext is MainViewModel vm)
        {
            await vm.OpenLightboxCommand.ExecuteAsync(page);
        }
    }

    private static PageItemViewModel? FindPage(object? hit)
    {
        if (hit is not Visual visual)
            return null;
        foreach (var ancestor in visual.GetVisualAncestors().Prepend(visual))
        {
            if (ancestor is StyledElement { DataContext: PageItemViewModel page })
                return page;
        }
        return null;
    }

    private static double Distance(Point a, Point b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
