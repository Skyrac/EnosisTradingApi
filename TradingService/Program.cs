using Newtonsoft.Json;
using System;
using Utils;
using Utils.Candles.Models;
using Utils.Enums;
using Utils.Messages.Models;
using WebSocketSharp;

namespace TradingService
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("");
            using (var ws = new WebSocket("ws://localhost:4300/subscribe"))
            {
                ws.OnMessage += (sender, e) =>
                {
                    if(e.IsText && e.Data.Length > 0)
                    {
                        var messageType = JsonConvert.DeserializeObject<BaseMessage>(e.Data);
                        if (messageType.Type == Utils.Messages.Enums.EMessage.CandleServiceUpdate)
                        {
                            var data = JsonConvert.DeserializeObject<CandleServiceUpdateMessage>(e.Data);
                            if (data == null || data.IntervalCandles.Count == 0)
                            {
                                return;
                            }
                            Console.WriteLine("Recieved Intervals: {0}", data.IntervalCandles.Count);
                            foreach (var intervalCandle in data.IntervalCandles)
                            {
                                Console.WriteLine("Interval {0} got {1} candles", intervalCandle.Interval, intervalCandle.Candles.Count);
                            }
                        } else if(messageType.Type == Utils.Messages.Enums.EMessage.CandleServiceHistoryCandles)
                        {

                            var data = JsonConvert.DeserializeObject<CandleServiceHistoryCandlesMessage>(e.Data);
                            if (data == null || data.Candles.Count == 0)
                            {
                                return;
                            }
                            Console.WriteLine("Recieved History Candles for {0} intervals", data.Candles.Count);
                            foreach (var intervalCandle in data.Candles)
                            {
                                foreach(var symbolCandle in intervalCandle.Symbols)
                                {
                                    Console.WriteLine("Recieved {0} Candles for {1} - {2}", symbolCandle.Klines.Count, symbolCandle.Symbol, intervalCandle.Interval);
                                }
                            }
                        }
                    }
                };
                ws.OnClose += (sender, e) =>
                {
                    ws.Close();
                };

                ws.Connect();
                ws.Send(JsonConvert.SerializeObject(new CandleServiceSubscriptionMessage(EExchange.BinanceSpot, 200, new WrappedIntervalCandles(Binance.Net.Enums.KlineInterval.OneMinute, new System.Collections.Generic.List<WrappedSymbolCandle>()
                {
                    { new WrappedSymbolCandle("ETHUSDT", null) },
                    { new WrappedSymbolCandle("BTCUSDT", null) },
                    { new WrappedSymbolCandle("BNBUSDT", null) }
                }))));
                Console.ReadKey();
            }
        }
    }
}
