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
    static readonly HttpClient client = new HttpClient();

    public PLGClient(string ipAddress, string macAddress = null, string password = "0", int port = 8080)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if(macAddress != null){
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

    }

    private async Task<ReceiveType> sendJsonPostRequest<SendType, ReceiveType>(string path, SendType message)
    {
        try
        {
            string json = JsonConvert.SerializeObject(message);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            string response = await sendRequest(path, content, HttpMethod.Post);

            // only return an object if we got content from the server
            if (response == null) return default!;
            ReceiveType result = JsonConvert.DeserializeObject<ReceiveType>(response)!;
            return result;
        }
        catch (Exception e)
        {
            throw new Exception($"Unknown error at {Address}{path}: {e.Message}");
        }



    }

    private async Task<string> sendRequest(string path, HttpContent? content, HttpMethod method)
    {
        try
        {
            var request = new HttpRequestMessage(method, "http://" + Address + path);
            request.Content = content;

            Console.WriteLine(Password);
            string header = "Bearer " + Password;
            Console.WriteLine(header);
            // request.Content = new ByteArrayContent()
            request.Headers.Add("Authorization", header);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            throw new Exception($"Could not send post request to {Address}{path}: {e.Message}");
        }
        catch (TaskCanceledException e)
        {
            throw new Exception($"Could not send post request to {Address}{path}: {e.Message}");
        }
        catch (Exception e)
        {
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
    }

    public async Task<bool> Ping()
    {
        string response = "";
        try
        {
            response = await sendRequest("/ping", null, HttpMethod.Get);
        }
        catch (Exception)
        {
            return false;
        }
        if (response == "pong") return true;
        return false;
    }

    public async Task DisplayText(string text)
    {
        try
        {
            if (text == null || text.Length == 0) throw new ArgumentException("Text cannot be null or empty");
            var message = new DisplayTextMessage { Text = text };
            await sendJsonPostRequest<DisplayTextMessage, object>("/displayText", message);
        }
        catch (Exception e)
        {
            throw new Exception($"Unknown error for {Address}: {e.Message}");
        }

    }

    public async Task<bool> ToggleBlackScreen()
    {
        var message = new object();
        ToggleBlackScreenReturnMessage result = await sendJsonPostRequest<object, ToggleBlackScreenReturnMessage>("/toggleBlackScreen", message);
        return result.BlackScreenEnabled;
    }

    public async Task RunCommand(string command)
    {
        if (command == null || command.Length == 0) throw new ArgumentException("Command cannot be null or empty");
        var message = new RunCommandMessage { Command = command };
        await sendJsonPostRequest<RunCommandMessage, object>("/runCommand", message);
    }

    public async Task OpenFile(string path)
    {
        if (path == null) throw new ArgumentException("Path cannot be null");
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
