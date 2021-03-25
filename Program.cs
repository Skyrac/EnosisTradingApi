using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using WebSocketSharp;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var ws = new WebSocket("ws://localhost/user"))
            {
                ws.OnMessage += (sender, e) =>
                {
                    ws.Close();
                };

                ws.Connect();
                ws.Send("BALUS");
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
