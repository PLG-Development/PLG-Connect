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
            string message;

            try
            {
                var result = JsonSerializer.Deserialize<DisplayTextMessage>(ctx.Request.DataAsString);
                if (result == null)
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await ctx.Response.Send("ERROR: Invalid JSON");
                    return;
                }
                message = result.Text;
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
            await ctx.Response.Send("");
        }

        // Slide Routes
        static async Task OpenSlideRoute(HttpContextBase ctx)
        {
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
}
