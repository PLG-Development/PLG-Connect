using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Security.Cryptography;


namespace PLG_Connect_Network;

public enum ClientAction
{
    Nothing,
    DisplayText,
    ToggleBlackScreen,
    Shutdown,
    RunCommand,
    OpenFile,
    NextSlide,
    PreviousSlide,
    WakeOnLAN,
    Ping,
    Plugin,
}

public class PLGClient
{
    public string Address { get; set; }
    public string? MacAddress { get; set; }
    public string Password;
    static readonly HttpClient client = new();
    public bool ShowsBlackScreen { get; set; }
    public ClientAction LastSuccessfulAction = ClientAction.Nothing;

    public PLGClient(string ipAddress, string? macAddress = null, string password = "0", int port = 8080)
    {
        Logger.Log("Welcome to PLG Connect Network Client!");
        Logger.Log("Starting up...");
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (macAddress != null)
        {
            if (!Regex.IsMatch(macAddress, macAddressPattern))
            {
                throw new ArgumentException("Invalid MAC address format");
            }
        }

        Address = ipAddress + ":" + port;
        if (macAddress != null)
        {
            MacAddress = macAddress.Replace(":", "-");
        }
        else
        {
            MacAddress = null;
        }

        Logger.Log("Successfully initialized client network");

    }

    private async Task<ReceiveType> SendJsonPostRequest<SendType, ReceiveType>(string path, SendType message)
    {
        string json = JsonConvert.SerializeObject(message);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        string response = await SendRequest(path, content, HttpMethod.Post);

        // only return an object if we got content from the server
        if (response == null) return default!;
        ReceiveType result = JsonConvert.DeserializeObject<ReceiveType>(response)!;
        return result;
    }

    private async Task<string> SendRequest(string path, HttpContent? content, HttpMethod method)
    {
        var request = new HttpRequestMessage(method, "http://" + Address + path);
        request.Content = content;

        string header = "Bearer " + Password;
        request.Headers.Add("Authorization", header);

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    public void SendWakeOnLAN()
    {
        if (MacAddress == null)
        {
            Logger.Log("No MAC configured :/", Logger.LogType.Error);
            return;
        }
        PhysicalAddress.Parse(MacAddress).SendWol();
        Logger.Log("WOL sent to " + MacAddress);
        LastSuccessfulAction = ClientAction.WakeOnLAN;
    }

    public async Task<bool> Ping()
    {
        string response;
        try
        {
            response = await SendRequest("/ping", null, HttpMethod.Get);
        }
        catch
        {
            return false;
        }

        LastSuccessfulAction = ClientAction.Ping;

        if (response == "pong")
        {
            return true;
        };
        return false;
    }

    public async Task DisplayText(string text)
    {
        if (text == null || text.Length == 0)
        {
            Logger.Log($"Error at {Address} while displaying text: Text cannot be null or empty", Logger.LogType.Error);
            throw new ArgumentException("Text cannot be null or empty");
        }
        var message = new DisplayTextMessage { Text = text };
        await SendJsonPostRequest<DisplayTextMessage, object>("/displayText", message);
        Logger.Log($"Displayed text to {Address}: {text}");

        LastSuccessfulAction = ClientAction.DisplayText;
    }

    public async Task<bool> ToggleBlackScreen()
    {
        var message = new object();
        ToggleBlackScreenReturnMessage result = await SendJsonPostRequest<object, ToggleBlackScreenReturnMessage>("/toggleBlackScreen", message);
        Logger.Log($"Toggled blackscreen at {Address}");
        ShowsBlackScreen = result.BlackScreenEnabled;
        LastSuccessfulAction = ClientAction.ToggleBlackScreen;
        return ShowsBlackScreen;
    }

    public async Task Shutdown()
    {
        await SendRequest("/shutdown", null, HttpMethod.Get);
        Logger.Log($"Powered off display at {Address}");
        LastSuccessfulAction = ClientAction.Shutdown;
    }

    public async Task RunCommand(string command)
    {
        if (command == null || command.Length == 0)
        {
            Logger.Log($"Error at {Address} while running command: Command cannot be null or empty", Logger.LogType.Error);
            throw new ArgumentException("Command cannot be null or empty");
        }
        var message = new RunCommandMessage { Command = command };
        await SendJsonPostRequest<RunCommandMessage, object>("/runCommand", message);
        Logger.Log($"Ran command at {Address}: {command}");
        LastSuccessfulAction = ClientAction.RunCommand;
    }

    private async Task SendFile(byte[] data, string fileExtension, string fileHash)
    {
        ByteArrayContent content = new(data);
        await SendRequest($"/sendFile?fileExtension={fileExtension}&fileHash={fileHash}", content, HttpMethod.Post);
    }
    private async Task JustOpenFile(string fileHash, string fileExtension)
    {
        var message = new object();
        await SendJsonPostRequest<object, object>($"/openFile?fileExtension={fileExtension}&fileHash={fileHash}", message);
    }
    private async Task<bool> HasFile(string fileHash, string fileExtension)
    {
        var message = new object();
        HasFileResponse response = await SendJsonPostRequest<object, HasFileResponse>($"/hasFile?fileExtension={fileExtension}&fileHash={fileHash}", message);
        return response.HasFile;
    }
    public async Task OpenFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException("Incorrect file path");
        }

        string fileExtension = Path.GetExtension(path).TrimStart('.').ToLower();
        byte[] fileData = File.ReadAllBytes(path);
        string fileHash = BitConverter.ToString(SHA1.Create().ComputeHash(fileData)).Replace("-", "").ToLower();

        Logger.Log("Checking file to open");
        bool displayHasFile = await HasFile(fileHash, fileExtension);
        if (!displayHasFile)
        {
            Logger.Log("Sending file to open");
            await SendFile(fileData, fileExtension, fileHash);
        }
        Logger.Log("Opened file");
        await JustOpenFile(fileHash, fileExtension);

        Logger.Log($"Opened file at {Address}: {path}");
        LastSuccessfulAction = ClientAction.OpenFile;
    }

    public async Task NextSlide()
    {
        var message = new object();
        await SendJsonPostRequest<object, object>("/nextSlide", message);
        Logger.Log($"Invoked next slide at {Address}");
        LastSuccessfulAction = ClientAction.NextSlide;
    }

    public async Task PreviousSlide()
    {
        var message = new object();
        await SendJsonPostRequest<object, object>("/previousSlide", message);
        Logger.Log($"Invoked previous slide at {Address}");
        LastSuccessfulAction = ClientAction.PreviousSlide;
    }


    /// <summary>
    /// message contains the data to be sent to the plugin including the plugin name
    /// </summary>
    /// <returns></returns>
    public async Task SendPluginRawData()
    {
        var message = new object();
        await SendJsonPostRequest<object, object>("/pluginCore", message);
        Logger.Log($"Sent plugin data to {Address}");
        LastSuccessfulAction = ClientAction.Plugin;
    }
}
