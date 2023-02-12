using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SapphireLauncherCore;

public sealed partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Save(object? sender, RoutedEventArgs e)
    {
        if (!Config.Instance.IsConfigValid(out string error))
        {
            ErrorPopup.Show(this, error);
            return;
        }

        Config.Instance.SaveConfig();
        Close();
    }

    private void Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}