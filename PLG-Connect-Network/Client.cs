using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace PLG_Connect_Network;


public class PLGClient
{
    public string Address { get; set; }
    public string MacAddress { get; set; }
    public string Password;
    static readonly HttpClient client = new HttpClient();

    public PLGClient(string ipAddress, string macAddress, string password, int port = 8080)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!Regex.IsMatch(macAddress, macAddressPattern))
        {
            throw new ArgumentException("Invalid MAC address format");
        }

        Password = password;
        Address = ipAddress+":"+port;
        MacAddress = macAddress.Replace(":", "-");
    }

    private async Task<ReceiveType> sendJsonPostRequest<SendType, ReceiveType>(string path, SendType message)
    {
        string json = JsonConvert.SerializeObject(message);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        string response = await sendRequest(path, content, HttpMethod.Post);

        // only return an object if we got content from the server
        if (response == null) return default!;
        ReceiveType result = JsonConvert.DeserializeObject<ReceiveType>(response)!;
        return result;
    }

    private async Task<string> sendRequest(string path, HttpContent? content, HttpMethod method)
    {
        try
        {
            var request = new HttpRequestMessage(method, "http://" + Address + path);
            request.Content = content;

            // request.Content = new ByteArrayContent()
            request.Headers.Add("Authorization", "Bearer " + Password);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            throw new Exception($"Could not send post request to {Address}{path}: {e.Message}");
        } catch (TaskCanceledException e) {
            throw new Exception($"Could not send post request to {Address}{path}: {e.Message}");
        }
    }

    public void SendWakeOnLAN()
    {
        PhysicalAddress.Parse(MacAddress).SendWol();
    }

    public async Task<bool> Ping()
    {
        string response = await sendRequest("/ping", null, HttpMethod.Get);
        if (response == "pong") return true;
        return false;
    }

    public async Task DisplayText(string text)
    {
        var message = new DisplayTextMessage { Text = text };
        await sendJsonPostRequest<DisplayTextMessage, object>("/displayText", message);
    }

    public async Task<bool> ToggleBlackScreen()
    {
        var message = new object();
        ToggleBlackScreenReturnMessage result = await sendJsonPostRequest<object, ToggleBlackScreenReturnMessage>("/toggleBlackScreen", message);
        return result.BlackScreenEnabled;
    }

    public async Task RunCommand(string command)
    {
        var message = new RunCommandMessage { Command = command };
        await sendJsonPostRequest<RunCommandMessage, object>("/runCommand", message);
    }

    public async Task OpenFile(string path)
    {
        string extension = Path.GetExtension(path).TrimStart('.').ToLower();

        byte[] fileBytes = File.ReadAllBytes(path);
        ByteArrayContent content = new ByteArrayContent(fileBytes);
        await sendRequest($"/openFile?fileEnding={extension}", content, HttpMethod.Post);
    }

    public async Task NextSlide()
    {
        var message = new object();
        await sendJsonPostRequest<object, object>("/nextSlide", message);
    }

    public async Task PreviousSlide()
    {
        var message = new object();
        await sendJsonPostRequest<object, object>("/previousSlide", message);
    }
}
