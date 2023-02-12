using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace SapphireLauncherCore;

public enum WineSyncMode
{
    None = 0,
    ESync = 1,
    FSync = 2
}

internal sealed class Config
{
    public static readonly Config Instance = new Config();

    public string GamePath { get; private set; } = string.Empty;
    public string WinePath { get; private set; } = string.Empty;
    public string WinePrefix { get; private set; } = string.Empty;
    public WineSyncMode SyncMode { get; private set; } = WineSyncMode.None;
    public string LoginURL { get; private set; } = string.Empty;
    public string AdditionalArgs { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public Language Language { get; private set; } = Language.English;

    public static bool SaveCredentials = true;

    public bool IsInitialized =>
        !string.IsNullOrWhiteSpace(GamePath) &&
        !string.IsNullOrWhiteSpace(WinePath) &&
        !string.IsNullOrWhiteSpace(WinePrefix) &&
        !string.IsNullOrWhiteSpace(LoginURL);

    private readonly static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

    private static readonly ImmutableDictionary<Type, Func<string, object>> ConfigValueParsers = new Dictionary<Type, Func<string, object>>
    {
        [typeof(string)] = static s => s,
        [typeof(int)] = static s => int.Parse(s),
        [typeof(WineSyncMode)] = static s => Enum.Parse<WineSyncMode>(s),
        [typeof(Language)] = static s => Enum.Parse<Language>(s)
    }.ToImmutableDictionary();

    // load from XML file or create new one
    public void Load()
    {
        if (!File.Exists(ConfigPath))
        {
            SaveConfig();
        }

        XElement config = XElement.Load(ConfigPath);

        GamePath = GetAttribute(nameof(GamePath), string.Empty);
        WinePath = GetAttribute(nameof(WinePath), string.Empty);
        WinePrefix = GetAttribute(nameof(WinePrefix), string.Empty);
        SyncMode = GetAttribute(nameof(SyncMode), WineSyncMode.None);
        LoginURL = GetAttribute(nameof(LoginURL), string.Empty);
        AdditionalArgs = GetAttribute(nameof(AdditionalArgs), string.Empty);
        Username = GetAttribute(nameof(Username), string.Empty);
        if (SaveCredentials)
        {
            Password = GetAttribute(nameof(Password), string.Empty);
            Language = GetAttribute(nameof(Language), Language.English);
        }

        T GetAttribute<T>(string name, T defaultValue)
        {
            XAttribute? field = config.Attribute(name.ToLower());
            if (field is null) { return defaultValue; }

            return (T)ConfigValueParsers[typeof(T)](field.Value);
        }
    }

    public bool IsConfigValid(out string error)
    {
        if (!File.Exists(GamePath))
        {
            error = "Game path is not valid.";
            return false;
        }

        if (!File.Exists(WinePath))
        {
            error = "Wine path is not valid.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    public void SaveConfig()
    {
        XElement root = new XElement("Config");

        AddField(nameof(GamePath), GamePath);
        AddField(nameof(WinePath), WinePath);
        AddField(nameof(WinePrefix), WinePrefix);
        AddField(nameof(SyncMode), SyncMode);
        AddField(nameof(LoginURL), LoginURL);
        AddField(nameof(AdditionalArgs), AdditionalArgs);
        if (SaveCredentials)
        {
            AddField(nameof(Username), Username);
            AddField(nameof(Password), Password);
        }
        AddField(nameof(Language), Language);

        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true,
            NewLineOnAttributes = true
        };

        using XmlWriter writer = XmlWriter.Create(ConfigPath, settings);

        root.WriteTo(writer);
        writer.Flush();

        void AddField<T>(string name, T defaultValue) => root.Add(new XAttribute(name.ToLower(), defaultValue?.ToString() ?? string.Empty));
    }
}