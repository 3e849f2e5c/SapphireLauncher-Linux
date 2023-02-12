#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SapphireLauncherCore;

internal enum LauncherResponseType
{
    Success,
    Error
}

internal record struct LauncherResponse(LauncherResponseType Type, string Message, Exception? Exception = null);

internal static class Launcher
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<LauncherResponse> Login()
    {
        if (!Config.Instance.IsInitialized)
        {
            return new LauncherResponse(LauncherResponseType.Error, "Please configure the launcher first.", null);
        }

        string send = CreateLoginMessage(Config.Instance.Username, Config.Instance.Password);

        LauncherResponse request = await SendRequest($"{Config.Instance.LoginURL}/sapphire-api/lobby/login", send);

        Config.Instance.SaveConfig();

        return OnLoginReceived(request.Message);
    }

    public static async Task<LauncherResponse> Register()
    {
        string send = CreateLoginMessage(Config.Instance.Username, Config.Instance.Password);

        LauncherResponse request = await SendRequest($"{Config.Instance.LoginURL}/sapphire-api/lobby/createAccount", send);

        return request.Type switch
        {
            LauncherResponseType.Error => request,
            _ => new LauncherResponse(LauncherResponseType.Success, request.Message)
        };
    }

    private static async Task<LauncherResponse> SendRequest(string url, string data)
    {
        HttpResponseMessage response;

        try
        {
            response = await client.PostAsync(url, new StringContent(data, Encoding.UTF8, "application/json"));
        }
        catch (HttpRequestException exception)
        {
            return new LauncherResponse(LauncherResponseType.Error, exception.Message, exception);
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            return new LauncherResponse(LauncherResponseType.Error, $"API response failed with code: {response.StatusCode}", null);
        }

        string responseString = response.Content.ReadAsStringAsync().Result;

        return new LauncherResponse(LauncherResponseType.Success, responseString);
    }

    private static string CreateLoginMessage(string username, string password)
    {
        return $"{{\"username\": \"{username}\",\"pass\":\"{password}\"}}";
    }

    private static LauncherResponse OnLoginReceived(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new LauncherResponse(LauncherResponseType.Error, "API responded with nothing.");
        }

        JsonNode? jsonObject = JsonNode.Parse(content);
        if (jsonObject is null)
        {
            return new LauncherResponse(LauncherResponseType.Error, "Unable to parse JSON data.");
        }

        JsonNode? sID = jsonObject["sId"],
                  lobbyHost = jsonObject["lobbyHost"],
                  frontierHost = jsonObject["frontierHost"];

        if (sID is null || lobbyHost is null || frontierHost is null)
        {
            return new LauncherResponse(LauncherResponseType.Error, $"API responded with invalid message: {jsonObject}");
        }

        Game.LaunchGame(
            sID.ToString(),
            ExpansionLevel.Ex1,
            Config.Instance.Language,
            lobbyHost.ToString(),
            frontierHost.ToString(),
            Config.Instance.AdditionalArgs);

        return new LauncherResponse(LauncherResponseType.Success, string.Empty);
    }
}