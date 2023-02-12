using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace SapphireLauncherCore;

public sealed partial class ErrorPopup : Window
{

    public ErrorPopup()
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

    public static void Show(Window? parent, string text)
    {
        var msgbox = new ErrorPopup();

        msgbox.FindControl<TextBlock>("Text").Text = text;

        if (parent != null)
        {
            msgbox.ShowDialog(parent);

            msgbox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            msgbox.Position = new PixelPoint(
                (int)(parent.Position.X + (parent.Width - msgbox.Width) / 2f),
                (int)(parent.Position.Y + (parent.Height - msgbox.Height) / 2f));
        }
        else
        {
            msgbox.Show();
        }
    }

    private void Close(object? sender, RoutedEventArgs e) => Close();
}