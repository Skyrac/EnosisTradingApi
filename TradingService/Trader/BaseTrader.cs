using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utils;
using Utils.Candles.Models;
using Utils.Clients;
using Utils.Enums;
using Utils.Messages.Enums;
using Utils.Messages.Models;
using Utils.Strategies;
using WebSocketSharp;

namespace TradingService.Trader
{
    public class BaseTrader
    {
        protected EExchange _exchange;
        protected bool _paperTest;
        private List<BaseClient> _clients = new List<BaseClient>();
        private Dictionary<string, Strategy> _strategies = new Dictionary<string, Strategy>();
        private BaseClient _defaultClient;
        private const string keyString = "{0}_{1}";
        private Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> _candles = new Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>>();
        private Dictionary<string, int> _requiredCandlesPerSymbol = new Dictionary<string, int>();
        private WebSocket _socket = new WebSocket("ws://localhost:4300/subscribe");
        public BaseTrader(BaseClient defaultClient, EExchange exchange)
        {
            _defaultClient = defaultClient;
            _socket.OnMessage += RecieveMessage;
            _socket.Connect();
        }

        private void RecieveMessage(object sender, MessageEventArgs e)
        {
            if (e.IsText && e.Data.Length > 0)
            {
                var message = JsonConvert.DeserializeObject<BaseMessage>(e.Data);
                if (message == null)
                {
                    Console.WriteLine("Unprovided message");
                    return;
                }
                HandleMessage(message.Type, e.Data);
            }
        }

        private void HandleMessage(EMessage type, string rawData)
        {
            Console.WriteLine("Recieved Message of Type {0}", type);
            switch (type)
            {
                case EMessage.CandleServiceSubscription:
                    break;
                case EMessage.CandleServiceUpdate:
                    var data = JsonConvert.DeserializeObject<CandleServiceUpdateMessage>(rawData);
                    if (data == null || data.IntervalCandles.Count == 0)
                    {
                        return;
                    }
                    Console.WriteLine("Recieved Intervals: {0}", data.IntervalCandles.Count);
                    foreach (var intervalCandle in data.IntervalCandles)
                    {
                        Console.WriteLine("Interval {0} got {1} candles", intervalCandle.Interval, intervalCandle.Candles.Count);
                    }
                    break;
                case EMessage.CandleServiceHistoryCandles:
                    var historyCandles = JsonConvert.DeserializeObject<CandleServiceHistoryCandlesMessage>(rawData);
                    if (historyCandles == null || historyCandles.Candles.Count == 0)
                    {
                        return;
                    }
                    Console.WriteLine("Recieved History Candles for {0} intervals", historyCandles.Candles.Count);
                    foreach (var intervalCandle in historyCandles.Candles)
                    {
                        foreach (var symbolCandle in intervalCandle.Symbols)
                        {
                            Console.WriteLine("Recieved {0} Candles for {1} - {2}", symbolCandle.Klines.Count, symbolCandle.Symbol, intervalCandle.Interval);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Recieved Message with Unknown EMessage-Type");
                    break;
            }
        }

        public void AddStrategy(Strategy strategy)
        {
            var key = string.Format(keyString, strategy.Name, strategy.Id);
            if (_strategies.ContainsKey(key))
            {
                return;
            }
            _strategies.Add(key, strategy);
            foreach (var wrappedInterval in strategy.RequiredIntervalSymbols)
            {
                foreach(var wrappedSymbol in wrappedInterval.Candles)
                if(!_requiredCandlesPerSymbol.ContainsKey(wrappedSymbol.Symbol))
                {
                    _requiredCandlesPerSymbol.Add(wrappedSymbol.Symbol, strategy.RequiredCandles);
                } else
                {
                    _requiredCandlesPerSymbol[wrappedSymbol.Symbol] = Math.Max(strategy.RequiredCandles, _requiredCandlesPerSymbol[wrappedSymbol.Symbol]);
                }
            }
            _socket.Send(JsonConvert.SerializeObject(new CandleServiceSubscriptionMessage(EExchange.BinanceSpot, strategy.RequiredCandles, strategy.RequiredIntervalSymbols.ToArray())));
        }



        public void RemoveStrategy(Strategy strategy)
        {
            var key = string.Format(keyString, strategy.Name, strategy.Id);
            if (!_strategies.ContainsKey(key))
            {
                return;
            }
        }
    }
}
