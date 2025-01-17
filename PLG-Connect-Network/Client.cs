using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace PLG_Connect_Network;


public class PLGClient
{
    public string Address { get; set; }
    public string? MacAddress { get; set; }
    public string Password;
    static readonly HttpClient client = new();
    public bool ShowsBlackScreen { get; set; }

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

        Password = password; Console.WriteLine(Password);
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
        try
        {
            string json = JsonConvert.SerializeObject(message);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            string response = await SendRequest(path, content, HttpMethod.Post);

            // only return an object if we got content from the server
            if (response == null) return default!;
            ReceiveType result = JsonConvert.DeserializeObject<ReceiveType>(response)!;
            Logger.Log($"Sent JSONPostRequest to {Address}{path}: {message}");
            return result;
        }
        catch (Exception e)
        {
            Logger.Log($"Unknown error at {Address}{path} while sending JSONPostRequest: {e.Message}", Logger.LogType.Error);
            throw new Exception($"Unknown error at {Address}{path}: {e.Message}");
        }
    }

    private async Task<string> SendRequest(string path, HttpContent? content, HttpMethod method)
    {
        try
        {
            var request = new HttpRequestMessage(method, "http://" + Address + path);
            request.Content = content;

            //Console.WriteLine(Password);
            string header = "Bearer " + Password;
            //Console.WriteLine(header);
            // request.Content = new ByteArrayContent()
            request.Headers.Add("Authorization", header);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            Logger.Log($"Unknown error at {Address}{path} while sending JSONPostRequest: {e.Message}", Logger.LogType.Error);
            throw new Exception($"Could not send post request to {Address}{path}: {e.Message}");
        }
        catch (TaskCanceledException e)
        {
            Logger.Log($"Unknown error at {Address}{path} while sending JSONPostRequest: {e.Message}", Logger.LogType.Error);
            throw new Exception($"Could not send post request to {Address}{path}: {e.Message}");
        }
        catch (Exception e)
        {
            Logger.Log($"Unknown error at {Address}{path} while sending JSONPostRequest: {e.Message}", Logger.LogType.Error);
            throw new Exception($"Unknown error at {Address}{path}: {e.Message}");
        }
    }

    public void SendWakeOnLAN()
    {
        if (MacAddress == null)
        {
            Console.WriteLine("No MAC configured :/");
            return;
        }
        PhysicalAddress.Parse(MacAddress).SendWol();
        Logger.Log("WOL sent to " + MacAddress);
    }

    public async Task<bool> Ping()
    {
        string response;
        try
        {
            response = await SendRequest("/ping", null, HttpMethod.Get);
        }
        catch (Exception ex)
        {
            Logger.Log($"Unknown error at {Address} while pinging: {ex.Message}", Logger.LogType.Error);
            return false;
        }

        if (response == "pong")
        {
            return true;
        };
        return false;
    }

    public async Task DisplayText(string text)
    {
        try
        {
            if (text == null || text.Length == 0)
            {
                Logger.Log($"Error at {Address} while displaying text: Text cannot be null or empty", Logger.LogType.Error);
                throw new ArgumentException("Text cannot be null or empty");
            }
            var message = new DisplayTextMessage { Text = text };
            await SendJsonPostRequest<DisplayTextMessage, object>("/displayText", message);
            Logger.Log($"Displayed text to {Address}: {text}");
        }
        catch (Exception e)
        {
            Logger.Log($"Unknown error at {Address} while displaying text: {e.Message}", Logger.LogType.Error);
            //throw new Exception($"Unknown error for {Address}: {e.Message}");
        }
    }

    public async Task<bool> ToggleBlackScreen()
    {
        var message = new object();
        ToggleBlackScreenReturnMessage result = await SendJsonPostRequest<object, ToggleBlackScreenReturnMessage>("/toggleBlackScreen", message);
        Logger.Log($"Toggled blackscreen at {Address}");
        ShowsBlackScreen = result.BlackScreenEnabled;
        return ShowsBlackScreen;
    }

    public async Task Shutdown()
    {
        try
        {
            var message = new object();
            await SendRequest("/shutdown", null, HttpMethod.Get);
            Logger.Log($"Powered off display at {Address}");
        }
        catch (Exception ex)
        {
            Logger.Log("Unknown error at Shutdown(): " + ex.Message);
        }
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
    }

    public async Task OpenFile(string path)
    {
        if (path == null)
        {
            Logger.Log($"Error at {Address} while opening file: Path cannot be null or empty", Logger.LogType.Error);
            throw new ArgumentException("Path cannot be null");

        }
        string extension = Path.GetExtension(path).TrimStart('.').ToLower();

        byte[] fileBytes = File.ReadAllBytes(path);
        ByteArrayContent content = new ByteArrayContent(fileBytes);
        await SendRequest($"/openFile?fileEnding={extension}", content, HttpMethod.Post); // type is needed to examine the controlling surface wether its internal or external
        Logger.Log($"Opened file at {Address}: {path}");
    }

    public async Task NextSlide()
    {
        var message = new object();
        await SendJsonPostRequest<object, object>("/nextSlide", message);
        Logger.Log($"Invoked next slide at {Address}");
    }

    public async Task PreviousSlide()
    {
        var message = new object();
        await SendJsonPostRequest<object, object>("/previousSlide", message);
        Logger.Log($"Invoked previous slide at {Address}");
    }
}
