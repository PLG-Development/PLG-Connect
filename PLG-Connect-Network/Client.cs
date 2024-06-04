using System.Text.Json;
using System.Text;


namespace PLG_Connect_Network;


public class Client
{
    public required string ServerAddress { get; set; }
    static readonly HttpClient client = new HttpClient();

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
            return;
        }
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
