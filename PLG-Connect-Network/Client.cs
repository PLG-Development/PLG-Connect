using System.Text.Json;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;


namespace PLG_Connect_Network;


public class ClientConnection
{
    public string IpAddress { get; set; }
    public string MacAddress { get; set; }
    public string Password;
    static readonly HttpClient client = new HttpClient();

    public ClientConnection(string ipAddress, string macAddress, string password)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!Regex.IsMatch(macAddress, macAddressPattern))
        {
            throw new ArgumentException("Invalid MAC address format");
        }

        Password = password;
        IpAddress = ipAddress;
        MacAddress = macAddress.Replace(":", "-");
    }

    private async Task sendJsonPostRequest<T>(string path, T message)
    {
        string json = JsonSerializer.Serialize(message);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        await sendPostRequest(path, content);
    }

    private async Task sendPostRequest(string path, HttpContent content)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://" + IpAddress + path);
            request.Content = content;

            // request.Content = new ByteArrayContent()
            request.Headers.Add("Authorization", "Bearer " + Password);

            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e.Message);
            return;
        } catch (TaskCanceledException e) {
            Console.WriteLine(e.Message);
            return;
        }
    }

    public void SendWakeOnLAN()
    {
        PhysicalAddress.Parse(MacAddress).SendWol();
    }

    public async Task DisplayText(string text)
    {
        var message = new DisplayTextMessage { Text = text };
        await sendJsonPostRequest<DisplayTextMessage>("/displayText", message);
    }

    public async Task ToggleBlackScreen()
    {
        var message = new object();
        await sendJsonPostRequest<object>("/toggleBlackScreen", message);
    }

    public async Task RunCommand(string command)
    {
        var message = new RunCommandMessage { Command = command };
        await sendJsonPostRequest<RunCommandMessage>("/runCommand", message);
    }

    public async Task OpenFile(string path)
    {
        string extension = Path.GetExtension(path).TrimStart('.').ToLower();

        byte[] fileBytes = File.ReadAllBytes(path);
        ByteArrayContent content = new ByteArrayContent(fileBytes);
        await sendPostRequest($"/openFile?fileEnding={extension}", content);
    }

    public async Task NextSlide()
    {
        var message = new object();
        await sendJsonPostRequest<object>("/nextSlide", message);
    }

    public async Task PreviousSlide()
    {
        var message = new object();
        await sendJsonPostRequest<object>("/previousSlide", message);
    }
}
