using System;
using System.Threading.Tasks;
using WatsonWebserver;
using WatsonWebserver.Core;
using System.Text.Json;
using System.Net;

namespace PLG_Connect_Presenter
{
    public class NetworkHandler
    {
        public NetworkHandler()
        {
            WebserverSettings settings = new()
            {
                Hostname = "127.0.0.1",
                Port = 8080,
            };
            Webserver server = new Webserver(settings, DefaultRoute);

            server.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/ping", PingRoute);
            server.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/displayText", DisplyTextRoute);
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
            await ctx.Response.Send("Hello from the default route!");
        }

        static async Task PingRoute(HttpContextBase ctx)
        {
            await ctx.Response.Send("pong");
        }

        // General purpose routes
        static async Task DisplyTextRoute(HttpContextBase ctx)
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

            await ctx.Response.Send("");
        }

        static async Task ToggleBlackScreenRoute(HttpContextBase ctx)
        {
            await ctx.Response.Send("");
        }
        static async Task RunCommandRoute(HttpContextBase ctx)
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

            await ctx.Response.Send("");
        }

        // Slide Routes
        static async Task OpenSlideRoute(HttpContextBase ctx)
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

            await ctx.Response.Send("");
        }

        static async Task NextSlideRoute(HttpContextBase ctx)
        {
            await ctx.Response.Send("");
        }

        static async Task PreviousSlideRoute(HttpContextBase ctx)
        {
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
