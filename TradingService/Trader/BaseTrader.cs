using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Candles.Models;
using Utils.Clients;
using Utils.Enums;
using Utils.Messages.Enums;
using Utils.Messages.Models;
using Utils.Strategies;
using Utils.Trading;
using Utils.Trading.Enums;
using WebSocketSharp;

namespace TradingService.Trader
{
    public class BaseTrader
    {
        protected EExchange _exchange;
        protected bool _paperTest;
        private List<BaseClient> _clients = new List<BaseClient>();
        private Dictionary<string, Strategy> _strategies = new Dictionary<string, Strategy>();
        private Dictionary<string, List<string>> _strategyUsers = new Dictionary<string, List<string>>();
        private Dictionary<string, BaseClient> _registeredUsers = new Dictionary<string, BaseClient>();
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
            switch (type)
            {
                case EMessage.CandleServiceSubscription:
                    break;
                case EMessage.CandleServiceUpdate:
                    if(TradingMessageHandler.HandleCandleServiceUpdateAndCheckForNewCandle(rawData, ref _candles, _strategies))
                    {
                        Console.WriteLine("Check for Entrance");
                        foreach(var strategyNames in _strategies.Keys)
                        {
                            var strategy = _strategies[strategyNames];
                            strategy.SetupIndicators(_candles);
                            var tradeInfos = strategy.EntryStrategy.EnterTrade(_candles, -1);
                            foreach(var info in tradeInfos)
                            {
                                Console.WriteLine("{0}: ENTER TRADE ON {1}", info.Opened, info.Symbol);
                            }
                        }
                    }
                    
                    break;
                case EMessage.CandleServiceHistoryCandles:
                    TradingMessageHandler.HandleCandleServiceHistoryCandles(rawData, ref _candles, _strategies);

                    foreach (var strategyNames in _strategies.Keys)
                    {
                        var strategy = _strategies[strategyNames];
                        strategy.SetupIndicators(_candles);
                        for(var i = 178; i < 399; i++)
                        {
                            var infos = strategy.EntryStrategy.EnterTrade(_candles, i);
                            foreach(var info in infos)
                            {
                                Console.WriteLine("{3}: Entered Trade on {0} with Stop = {1} and TP = {2}", info.Symbol, info.StopLoss, info.TakeProfit, info.Opened);        //NOTE: Wie komme ich an aktuellen Preis? :)
                            }
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
            var wrappedIntervalSymbols = new List<WrappedIntervalCandles>();
            foreach(var interval in strategy.GetRequiredIntervalCandles())
            {
                var wrappedSymbols = new List<WrappedSymbolCandle>();
                foreach(var symbol in interval.Value.Where(item => !string.IsNullOrEmpty(item)))
                {
                    if (!_requiredCandlesPerSymbol.ContainsKey(symbol))
                    {
                        _requiredCandlesPerSymbol.Add(symbol, strategy.RequiredCandles);
                    }
                    else
                    {
                        _requiredCandlesPerSymbol[symbol] = Math.Max(strategy.RequiredCandles, _requiredCandlesPerSymbol[symbol]);
                    }
                    wrappedSymbols.Add(new WrappedSymbolCandle(symbol, null));
                }

                wrappedIntervalSymbols.Add(new WrappedIntervalCandles(interval.Key, wrappedSymbols));
            }
            _socket.Send(JsonConvert.SerializeObject(new CandleServiceSubscriptionMessage(EExchange.BinanceSpot, strategy.RequiredCandles, wrappedIntervalSymbols.ToArray())));
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
