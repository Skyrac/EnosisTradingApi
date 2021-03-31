using System;
using TradingService.Trader;
using Utils.Clients;
using Utils.Enums;

namespace TradingService
{
    class Program
    {
        static void Main(string[] args)
        {
            new BaseTrader(new BinanceBaseClient("", ""), EExchange.BinanceSpot);
            Console.ReadKey();
        }
    }
}
