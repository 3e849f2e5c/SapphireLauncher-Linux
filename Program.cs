using Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SapphireLauncherCore;

internal sealed class Program
{
    private static bool terminate;

    [STAThread]
    public static void Main(string[] args)
    {
        bool autoLaunch = false;

        foreach (string s in args)
        {
            if (s is "--") { break; }

            switch (s)
            {
                case "--help" or "-h":
                    PrintHelp();
                    return;
                case "--auto-login" or "-a":
                    autoLaunch = true;
                    break;
                case "--version" or "-v":
                    Console.WriteLine($"Sapphire Launcher Linux {FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion}");
                    return;
                case "--no-save-credentials" or "-n":
                    Config.SaveCredentials = false;
                    break;
            }
        }

        Config.Instance.Load();

        if (autoLaunch)
        {
            if (!Config.SaveCredentials)
            {
                Console.WriteLine("Cannot auto-login without saving credentials.");
                return;
            }

            AutoLaunch();
            while (!terminate)
            {
                if (Game.ActiveProcesses.Count > 0 && Game.ActiveProcesses.All(static x => x.HasExited))
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            return;
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void PrintHelp() =>
        Console.WriteLine("Sapphire Launcher Linux\n" +
                          "Usage: SapphireLauncherLinux [options]\n" +
                          "Options:\n" +
                          "  --help, -h\t\t\tShow this help message.\n" +
                          "  --version, -v\t\t\tShow version information.\n" +
                          "  --auto-login, -a\t\tAutomatically login to the game.\n" +
                          "  --no-save-credentials, -n\tDo not save credentials.\n");

    private static async void AutoLaunch()
    {
        LauncherResponse response = await Launcher.Login();
        if (response.Type is LauncherResponseType.Success) { return; }

        Console.WriteLine($"{response.Message}");
        terminate = true;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .LogToTrace();
}