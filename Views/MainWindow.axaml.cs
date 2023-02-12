using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SapphireLauncherCore;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Login(object? sender, RoutedEventArgs e)
    {
        LauncherResponse response = await Launcher.Login();

        if (response.Type is LauncherResponseType.Success) { return; }

        ErrorPopup.Show(this, response.Message);
        if (response.Exception is null) { return; }

        Console.WriteLine(response.Exception);
    }

    private async void Register(object? sender, RoutedEventArgs e)
    {
        LauncherResponse response = await Launcher.Register();

        if (response.Type is LauncherResponseType.Success)
        {
            ErrorPopup.Show(this, "Registration successful.");
            return;
        }

        ErrorPopup.Show(this, response.Message);
        if (response.Exception is null) { return; }

        Console.WriteLine(response.Exception);
    }

    private void OpenConfig(object? sender, RoutedEventArgs e)
    {
        ConfigWindow window = new ConfigWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            DataContext = Config.Instance
        };

        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Position = new PixelPoint(
            (int)(Position.X + (Width - window.Width) / 2f),
            (int)(Position.Y + (Height - window.Height) / 2f));

        window.ShowDialog(this);
    }
}