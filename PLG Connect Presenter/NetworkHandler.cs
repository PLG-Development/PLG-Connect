using System;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;
using System.Text.Json;
using System.Net;
using System.Collections.Generic;

namespace PLG_Connect_Network
{
    public class Server
    {
        public List<Action<string>> displayTextHandlers = new List<Action<string>>();
        public List<Action> toggleBlackScreenHandlers = new List<Action>();
        public List<Action<string>> runCommandHandlers = new List<Action<string>>();
        public List<Action<string>> openSlideHandlers = new List<Action<string>>();
        public List<Action> nextSlideHandlers = new List<Action>();
        public List<Action> previousSlideHandlers = new List<Action>();

        public Server(int port = 8080)
        {
            WebserverSettings settings = new()
            {
                Hostname = "127.0.0.1",
                Port = port,
            };
            Webserver server = new Webserver(settings, DefaultRoute);

            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/ping", PingRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/displayText", DisplayTextRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/toggleBlackScreen", ToggleBlackScreenRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/runCommand", RunCommandRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/openSlide", OpenSlideRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/nextSlide", NextSlideRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/previousSlide", PreviousSlideRoute);

            server.StartAsync();
        }

        static T ExtractObject<T>(HttpContextBase ctx)
        {
            var result = JsonSerializer.Deserialize<T>(ctx.Request.DataAsString);
            if (result == null)
            {
                throw new Exception("Invalid JSON");
            }

            return result;
        }

        static async Task DefaultRoute(HttpContextBase ctx)
        {
            await ctx.Response.Send("This should never be called");
        }

        static async Task PingRoute(HttpContextBase ctx)
        {
            await ctx.Response.Send("Pong");
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
                await ctx.Response.Send($"ERROR: {e.Message}");
                return;
            }

            foreach (var handler in displayTextHandlers)
            {
                handler(result.Text);
            }
            await ctx.Response.Send("");
        }

        async Task ToggleBlackScreenRoute(HttpContextBase ctx)
        {
            foreach (var handler in toggleBlackScreenHandlers)
            {
                handler();
            }
            await ctx.Response.Send("");
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

        // Slide Routes
        async Task OpenSlideRoute(HttpContextBase ctx)
        {
            OpenSlideMessage result;

            try
            {
                result = ExtractObject<OpenSlideMessage>(ctx);
            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await ctx.Response.Send($"ERROR: {e.Message}");
                return;
            }

            foreach (var handler in openSlideHandlers)
            {
                handler(result.SlidePath);
            }
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

    public class DisplayTextMessage
    {
        public required string Text { get; set; }
    }

    public class RunCommandMessage
    {
        public required string Command { get; set; }
    }

    public class OpenSlideMessage
    {
        public required string SlidePath { get; set; }
    }
}
