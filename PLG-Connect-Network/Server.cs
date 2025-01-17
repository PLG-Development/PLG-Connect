using System.Net;
using WatsonHttpMethod = WatsonWebserver.Core.HttpMethod;
using WatsonWebserver;
using WatsonWebserver.Core;
using Newtonsoft.Json;


namespace PLG_Connect_Network;


public class PLGServer
{
    public List<Action<string>> displayTextHandlers = new List<Action<string>>();
    public List<Action> toggleBlackScreenHandlers = new List<Action>();
    public List<Action<string>> runCommandHandlers = new List<Action<string>>();
    public List<Action> nextSlideHandlers = new List<Action>();
    public List<Action> previousSlideHandlers = new List<Action>();
    public List<Action> firstRequestHandlers = new List<Action>();
    public List<Action> beforeRequestHandlers = new List<Action>();
    public List<Action<string>> openFileHandlers = new List<Action<string>>();
    public List<Action> shutdownHandlers = new List<Action>();
    public string Password;
    private string dataFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PLG-Development",
        "PLG-Connect-Presenter",
        "data"
    );

    private Webserver server;
    public PLGServer(string password = "0", int port = 8080)
    {
        Logger.Log("Welcome to PLG Connect Network Server!");
        Logger.Log("Starting up...");
        Password = password;

        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        WebserverSettings settings = new()
        {
            Hostname = "*",
            Port = port,
        };
        server = new Webserver(settings, DefaultRoute);

        server.Routes.PreAuthentication.Static.Add(WatsonHttpMethod.GET, "/ping", PingRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/changePassword", ChangePasswordRoute);

        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/displayText", DisplayTextRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/toggleBlackScreen", ToggleBlackScreenRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/runCommand", RunCommandRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/nextSlide", NextSlideRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/previousSlide", PreviousSlideRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.GET, "/shutdown", ShutdownRoute);

        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/openFile", OpenFileRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/sendFile", SendFileRoute);
        server.Routes.PostAuthentication.Static.Add(WatsonHttpMethod.POST, "/hasFile", HasFileRoute);

        server.Routes.PreRouting = BeforeRequest;
        server.Routes.AuthenticateRequest = AuthenticateRequest;

        server.StartAsync();

        Logger.Log("Successfully initialized server network");
    }

    public void Stop()
    {
        server.Stop();
        Logger.Log("Server stopped");
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
        Logger.Log("Successfully Authenticated!");
        return true;
    }

    static T ExtractObject<T>(HttpContextBase ctx)
    {
        var result = JsonConvert.DeserializeObject<T>(ctx.Request.DataAsString);
        if (result == null)
        {
            throw new Exception("Invalid JSON");
        }
        Logger.Log("JSON Object from HttpContextBase extracted");
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
            Logger.Log("Error while changing password: " + e.Message, Logger.LogType.Error);
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        Password = result.NewPassword;
        Logger.Log("Successfully changed password");
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
        Logger.Log("Ping answered: pong! |   o|");
    }

    // General purpose routes
    async Task DisplayTextRoute(HttpContextBase ctx)
    {
        DisplayTextMessage result;

        try
        {
            result = ExtractObject<DisplayTextMessage>(ctx);
        }
        catch (Exception e)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Error while displaying text: " + e.Message, Logger.LogType.Error);
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        foreach (var handler in displayTextHandlers)
        {
            handler(result.Text);
        }
        await ctx.Response.Send("");
        Logger.Log("Displayed text: " + result);
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
        Logger.Log("Toggled blackscreen");
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
            Logger.Log("Error while running command: " + e.Message, Logger.LogType.Error);
            await ctx.Response.Send($"ERROR: {e.Message}");
            return;
        }

        foreach (var handler in runCommandHandlers)
        {
            handler(result.Command);
            Logger.Log("Extracted something from anything: " + result.Command);
        }
        await ctx.Response.Send("");
        Logger.Log("Ran commands");
    }

    async Task ShutdownRoute(HttpContextBase ctx)
    {

        foreach (var handler in shutdownHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
        Logger.Log("Bye bye display: powered off display");
    }

    async Task HasFileRoute(HttpContextBase ctx)
    {
        string? fileHash = ctx.Request.Query.Elements.Get("fileHash");
        string? fileExtension = ctx.Request.Query.Elements.Get("fileExtension");
        if (fileHash == null || fileExtension == null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Got wrong has file route request", Logger.LogType.Error);
            await ctx.Response.Send("missing hash param");
            return;
        }
        string filePath = Path.Combine(dataFolderPath, fileHash) + $".{fileExtension}";

        bool fileExists = File.Exists(filePath);
        Logger.Log($"File exists: {fileExists}");

        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        await ctx.Response.Send(JsonConvert.SerializeObject(new HasFileResponse { HasFile = fileExists }));
    }

    async Task OpenFileRoute(HttpContextBase ctx)
    {
        string? fileHash = ctx.Request.Query.Elements.Get("fileHash");
        string? fileExtension = ctx.Request.Query.Elements.Get("fileExtension");
        if (fileHash == null || fileExtension == null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Got wrong has file route request", Logger.LogType.Error);
            await ctx.Response.Send("missing hash param");
            return;
        }

        string filePath = Path.Combine(dataFolderPath, fileHash) + $".{fileExtension}";
        if (!File.Exists(filePath))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("File does not exist", Logger.LogType.Error);
            await ctx.Response.Send("file does not exist");
            return;
        }

        foreach (var handler in openFileHandlers)
        {
            handler(filePath);
        }

        await ctx.Response.Send("");
        Logger.Log("Opened file");
    }

    async Task SendFileRoute(HttpContextBase ctx)
    {

        string? fileHash = ctx.Request.Query.Elements.Get("fileHash");
        string? fileExtension = ctx.Request.Query.Elements.Get("fileExtension");
        if (fileHash == null || fileExtension == null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("Got wrong has file route request", Logger.LogType.Error);
            await ctx.Response.Send("missing hash param");
            return;
        }

        string filePath = Path.Combine(dataFolderPath, fileHash) + $".{fileExtension}";
        if (File.Exists(filePath))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            Logger.Log("File already exist", Logger.LogType.Error);
            await ctx.Response.Send("file already exist");
            return;
        }

        await File.WriteAllBytesAsync(filePath, ctx.Request.DataAsBytes);

        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
        await ctx.Response.Send("");
        Logger.Log("Sent file");
    }

    async Task NextSlideRoute(HttpContextBase ctx)
    {
        foreach (var handler in nextSlideHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
        Logger.Log("Invoked next slide");
    }

    async Task PreviousSlideRoute(HttpContextBase ctx)
    {
        foreach (var handler in previousSlideHandlers)
        {
            handler();
        }
        await ctx.Response.Send("");
        Logger.Log("Invoked previous slide");
    }
}
