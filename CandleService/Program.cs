using CandleService.Exchanges;
using CandleService.Services;
using Utils.Enums;
using System;
using System.Collections.Generic;
using WebSocketSharp.Server;
using Utils.Strategies.Models;
using Utils.Indicators.Models;
using Utils.Strategies.Enums;

namespace CandleService
{
    public class Program
    {
        public static WebSocketServer Server;
        public static Dictionary<EExchange, BaseExchange> Exchanges = new Dictionary<EExchange, BaseExchange>();
        static void Main(string[] args)
        {
            Exchanges.Add(EExchange.BinanceSpot, new BinanceExchange(EExchange.BinanceSpot));
            Exchanges.Add(EExchange.BinanceFuturesUsdt, new BinanceExchange(EExchange.BinanceFuturesUsdt));

            Server = new WebSocketServer("ws://localhost:4300");
            Server.AddWebSocketService<Subscriber>("/subscribe");
            Server.Start();
            Console.WriteLine("SERVER STARTED LISTENING");
            Console.ReadKey();
        }

        private static void GenerateLongCondition()
        {
            var condition = new ConditionSequence();
            var subCondition = new ConditionSequence();
            condition.AddCondition(subCondition);
            subCondition.AddCondition(new ConditionNode()
            {
                FirstItem = new ConditionItem()
                {
                    Name = "EMA_8",
                    Interval = Binance.Net.Enums.KlineInterval.OneMinute,
                    Symbol = "ETHUSDT",
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 8)
                },
                Operator = BoolOperator.GreaterThan,
                SecondItem = new ConditionItem()
                {
                    Name = "EMA_21",
                    Interval = Binance.Net.Enums.KlineInterval.OneMinute,
                    Symbol = "ETHUSDT",
                    Index = 0,
                    Indicator = new IndicatorProperties("Ema", "GetEma", 21)
                }
            });
        }
    }
}
