using System;
using WebSocketSharp;

namespace TradingService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ws = new WebSocket("ws://localhost:4200/user"))
            {
                ws.OnMessage += (sender, e) =>
                {
                    Console.WriteLine(e.Data);
                };
                ws.OnClose += (sender, e) =>
                {
                    ws.Close();
                };

                ws.Connect();
                ws.Send("BALUS");
                Console.ReadKey();
            }
        }
    }
}
