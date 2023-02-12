using Avalonia;
using System;
using System.Linq;
using System.Threading;

namespace SapphireLauncherCore;

internal class Program
{
    public static bool Terminate;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Config.Instance.Load();

        if (args.Length > 0 && args[0] is "autolaunch")
        {
            AutoLaunch();
            while (!Terminate)
            {
                if (Game.ActiveProcesses.Count > 0 && Game.ActiveProcesses.All(static x => x.HasExited))
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            return;
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static async void AutoLaunch()
    {
        LauncherResponse response = await Launcher.Login();
        if (response.Type is LauncherResponseType.Success) { return; }
        Console.WriteLine($"{response.Message}");
        Terminate = true;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
