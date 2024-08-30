using System.Net;
using WatsonHttpMethod = WatsonWebserver.Core.HttpMethod;
using WatsonWebserver;
using WatsonWebserver.Core;
using System.Text.Json;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace PLG_Connect_Network;


public class Server
{
    public List<Action<string>> displayTextHandlers = new List<Action<string>>();
    public List<Action> toggleBlackScreenHandlers = new List<Action>();
    public List<Action<string>> runCommandHandlers = new List<Action<string>>();
    public List<Action> nextSlideHandlers = new List<Action>();
    public List<Action> previousSlideHandlers = new List<Action>();
    public List<Action> firstRequestHandlers = new List<Action>();
    public List<Action> beforeRequestHandlers = new List<Action>();
    public List<Action<string>> openFileHandlers = new List<Action<string>>();
    public string Password;

    private Webserver server;
    public Server(string password = "0", int port = 8080)
    {
        Password = password;

        WebserverSettings settings = new()
        {
            Hostname = "*",
            Port = port,
        };
        server = new Webserver(settings, DefaultRoute);

        server.Routes.PreAuthentication.Static.Add(WatsonHttpMethod.GET, "/ping", PingRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/changePassword", ChangePasswordRoute);

        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/displayText", DisplyTextRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/toggleBlackScreen", ToggleBlackScreenRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/runCommand", RunCommandRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/nextSlide", NextSlideRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/previousSlide", PreviousSlideRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/openFile", OpenFileRoute);

        server.Routes.PreRouting = BeforeRequest;
        server.Routes.AuthenticateRequest = AuthenticateRequest;

        server.StartAsync();
    }

    public void Stop()
    {
        server.Stop();
    }

    bool firstRequestHappend = false;
    Task BeforeRequest(HttpContextBase ctx)
    {
        foreach (var handler in beforeRequestHandlers)
        {
            handler();
        }

        if (firstRequestHappend) { return Task.CompletedTask; }

        foreach (var handler in firstRequestHandlers)
        {
            handler();
        }
        firstRequestHappend = true;
        return Task.CompletedTask;
    }

    async Task<bool> AuthenticateRequest(HttpContextBase ctx)
    {
        string? authHeader = ctx.Request.Headers["Authorization"];
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.Send("Unauthorized");
            return false;
        }

        string token = authHeader.Substring("Bearer ".Length);
        if (token != Password)
        {
            ctx.Response.StatusCode = 401;
            await ctx.Response.Send("Unauthorized");
            return false;
        }

        return true;
    }

    static T ExtractObject<T>(HttpContextBase ctx)
    {
        var result = JsonConvert.DeserializeObject<T>(ctx.Request.DataAsString);
        if (result == null)
        {
            throw new Exception("Invalid JSON");
        }

        return result;
    }

    async Task ChangePasswordRoute(HttpContextBase ctx)
    {
        ChangePasswordMessage result;

        try
        {
            result = ExtractObject<ChangePasswordMessage>(ctx);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        Password = result.NewPassword;
        await ctx.Response.Send("");

    }

    static async Task DefaultRoute(HttpContextBase ctx)
    {
        ctx.Response.StatusCode = 400;
        await ctx.Response.Send("This should never be called");
    }

    static async Task PingRoute(HttpContextBase ctx)
    {
        await ctx.Response.Send("pong");
    }

    // General purpose routes
    async Task DisplyTextRoute(HttpContextBase ctx)
    {
        DisplayTextMessage result;

        try
        {
            result = ExtractObject<DisplayTextMessage>(ctx);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        foreach (var handler in displayTextHandlers)
        {
            handler(result.Text);
        }
        await ctx.Response.Send("");
    }

    // note: this function handels the black screen state paralel to the actual presenter logic which duplicats the code and is not ideal
    // however this makes it much simpler to handler because the presenter need to work on different thread and therefore cant easily return values
    private bool blackScreenEnabled = false;
    async Task ToggleBlackScreenRoute(HttpContextBase ctx)
    {
        foreach (var handler in toggleBlackScreenHandlers)
        {
            handler();
        }
        blackScreenEnabled = !blackScreenEnabled;
        await ctx.Response.Send(JsonConvert.SerializeObject(new ToggleBlackScreenReturnMessage { BlackScreenEnabled = blackScreenEnabled }));
    }
    async Task RunCommandRoute(HttpContextBase ctx)
    {
        RunCommandMessage result;

        try
        {
            result = ExtractObject<RunCommandMessage>(ctx);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        foreach (var handler in runCommandHandlers)
        {
            handler(result.Command);
        }
        await ctx.Response.Send("");
    }

    async Task OpenFileRoute(HttpContextBase ctx)
    {
        string? fileEnding = ctx.Request.Query.Elements.Get("fileEnding");
        if (fileEnding == null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.Send("Missing fileEnding");
            return;
        }

        string fileHash = BitConverter.ToString(SHA1.Create().ComputeHash(ctx.Request.DataAsBytes)).Replace("-", "").ToLower();

        string folderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PLG-Connect"
        );
        // Create folder if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, fileHash) + $".{fileEnding}";

        // Only donwload file if it doesn't exist
        if (!File.Exists(filePath))
        {
            await File.WriteAllBytesAsync(filePath, ctx.Request.DataAsBytes);
        }

        foreach (var handler in openFileHandlers)
        {
            handler(filePath);
        }

        await ctx.Response.Send("");
    }

    async Task NextSlideRoute(HttpContextBase ctx)
    {
        foreach (var handler in nextSlideHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
    }

    async Task PreviousSlideRoute(HttpContextBase ctx)
    {
        foreach (var handler in previousSlideHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
    }
}
