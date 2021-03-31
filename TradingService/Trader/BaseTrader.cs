using Binance.Net.Enums;
using CryptoExchange.Net;
using System;
using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Enums;
using Utils.Strategies;

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
        public BaseTrader(BaseClient defaultClient, EExchange exchange)
        {
            _defaultClient = defaultClient;
        }

        public void AddStrategy(Strategy strategy)
        {
            var key = string.Format(keyString, strategy.Name, strategy.Id);
            if (_strategies.ContainsKey(key))
            {
                return;
            }
            _strategies.Add(key, strategy);
            //TODO: Subscribe to Candles
            foreach(var symbol in strategy.Symbols)
            {
                if(!_requiredCandlesPerSymbol.ContainsKey(symbol))
                {
                    _requiredCandlesPerSymbol.Add(symbol, strategy.RequiredCandles);
                } else
                {
                    _requiredCandlesPerSymbol[symbol] = strategy.RequiredCandles;
                }
            }

            //TODO: Recieve Required Candles
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
