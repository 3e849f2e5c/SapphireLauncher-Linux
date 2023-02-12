using System.Collections.Generic;
using System.Diagnostics;

namespace SapphireLauncherCore;

internal enum Language
{
    Japanese = 0,
    English = 1,
    German = 2,
    French = 3
}

internal enum ExpansionLevel
{
    /// <summary>
    /// ARR
    /// </summary>
    Base = 1,
    /// <summary>
    /// HW
    /// </summary>
    Ex1 = 2,
    /// <summary>
    /// SB
    /// </summary>
    Ex2 = 3,
    /// <summary>
    /// ShB
    /// </summary>
    Ex3 = 4,
    /// <summary>
    /// EW
    /// </summary>
    Ex4 = 5
}

internal static class Game
{
    public static readonly List<Process> ActiveProcesses = new List<Process>();

    public static void LaunchGame(string sId, ExpansionLevel expansion, Language language, string lobbyHost, string frontierHost, string additionalArgs)
    {
        string args = string.Format(
            "DEV.TestSID={0} DEV.UseSqPack=1 DEV.DataPathType=1 " +
            "DEV.LobbyHost01={1} DEV.LobbyPort01=54994 " +
            "DEV.LobbyHost02={1} DEV.LobbyPort02=54994 " +
            "DEV.LobbyHost03={1} DEV.LobbyPort03=54994 " +
            "DEV.LobbyHost04={1} DEV.LobbyPort04=54994 " +
            "DEV.LobbyHost05={1} DEV.LobbyPort05=54994 " +
            "DEV.LobbyHost06={1} DEV.LobbyPort06=54994 " +
            "DEV.LobbyHost07={1} DEV.LobbyPort07=54994 " +
            "DEV.LobbyHost08={1} DEV.LobbyPort08=54994 " +
            "SYS.Region=3 language={2} version=1.0.0.0 DEV.MaxEntitledExpansionID={3} DEV.GMServerHost={4} {5}",
            sId, lobbyHost, (int)language, (int)expansion, frontierHost, additionalArgs);

        var startInfo = new ProcessStartInfo
        {
            Arguments = $"\"{Config.Instance.GamePath}\" {args}",
            WorkingDirectory = Config.Instance.WinePrefix,
            FileName = Config.Instance.WinePath
        };
        startInfo.EnvironmentVariables.Add("WINEPREFIX", Config.Instance.WinePrefix);
        startInfo.EnvironmentVariables.Add("WINEARCH", "win64");

        switch (Config.Instance.SyncMode)
        {
            case WineSyncMode.FSync:
                startInfo.EnvironmentVariables.Add("WINEFSYNC", "1");
                break;
            case WineSyncMode.ESync:
                startInfo.EnvironmentVariables.Add("WINEESYNC", "1");
                break;
        }

        // We need to remove this if the key already exists or this will crash, yay!
        if (startInfo.EnvironmentVariables.ContainsKey("XMODIFIERS"))
        {
            startInfo.EnvironmentVariables.Remove("XMODIFIERS");
        }
        // Fix keyboard input
        startInfo.EnvironmentVariables.Add("XMODIFIERS", "@im=null");

        Process? process = Process.Start(startInfo);

        if (process is null) { return; }

        ActiveProcesses.Add(process);
    }
}