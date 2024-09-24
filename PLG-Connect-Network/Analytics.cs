namespace PLG_Connect_Network;


public static class Analytics
{
    public async static Task SendEvent(string action)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://" + "analytics.ugolis.de/track/plg-connect?action=" + action);
        var client = new HttpClient();

        try {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        } catch (Exception e) {
            Console.WriteLine($"Error sending analytics: {e.Message}");
        }
    }
}