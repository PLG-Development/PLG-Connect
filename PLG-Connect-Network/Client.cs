using System.Text.Json;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;


namespace PLG_Connect_Network;


public class ClientConnection
{
    public string ServerAddress { get; set; }
    public string MacAddress { get; set; }
    static readonly HttpClient client = new HttpClient();

    public ClientConnection(string serverAddress, string macAddress)
    {
        string macAddressPattern = @"^([0-9A-Fa-f]{2}:){5}([0-9A-Fa-f]{2})$";
        if (!Regex.IsMatch(macAddress, macAddressPattern))
        {
          throw new ArgumentException("Invalid MAC address format");
        }

        ServerAddress = serverAddress;
        MacAddress = macAddress.Replace(":", "-");
    }

    private async Task sendPostRequest<T>(string path, T message)
    {
        try
        {
            string json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(ServerAddress + path, content);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return;
        }
        catch (HttpRequestException e)
        {
#if DEBUG
            Console.WriteLine(e.Message);
#endif
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
        await sendPostRequest<DisplayTextMessage>("/displayText", message);
    }

    public async Task ToggleBlackScreen()
    {
        var message = new object();
        await sendPostRequest<object>("/toggleBlackScreen", message);
    }

    public async Task RunCommand(string command)
    {
        var message = new RunCommandMessage { Command = command };
        await sendPostRequest<RunCommandMessage>("/runCommand", message);
    }

    public async Task OpenSlide(string slidePath)
    {
        var message = new OpenSlideMessage { SlidePath = slidePath };
        await sendPostRequest<OpenSlideMessage>("/openSlide", message);
    }

    public async Task NextSlide()
    {
        var message = new object();
        await sendPostRequest<object>("/nextSlide", message);
    }

    public async Task PreviousSlide()
    {
        var message = new object();
        await sendPostRequest<object>("/previousSlide", message);
    }
}
