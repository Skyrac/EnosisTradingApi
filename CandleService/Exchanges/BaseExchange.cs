using Binance.Net.Enums;
using CandleService.Services;
using Utils.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils.Candles.Models;
using System.Threading;

namespace CandleService.Exchanges
{
    public abstract class BaseExchange
    {
        protected EExchange _exchange;

        protected Dictionary<string, Dictionary<KlineInterval, Dictionary<Subscriber, int>>> _listeners = new Dictionary<string, Dictionary<KlineInterval, Dictionary<Subscriber, int>>>();
        protected Dictionary<KlineInterval, Dictionary<string, KeyValuePair<Kline, bool>>> _candles = new Dictionary<KlineInterval, Dictionary<string, KeyValuePair<Kline, bool>>>();
        private Timer timer;
        public BaseExchange(EExchange exchange)
        {
            _exchange = exchange;
            timer = new Timer((_) => Publish(), null, 0, 1000);
        }

        private bool AddListener(string symbol, KlineInterval interval, Subscriber subscriber)
        {
            if(!_listeners.ContainsKey(symbol))
            {
                _listeners.Add(symbol, new Dictionary<KlineInterval, Dictionary<Subscriber, int>>());
            }
            if(!_listeners[symbol].ContainsKey(interval))
            {
                _listeners[symbol].Add(interval, new Dictionary<Subscriber, int>());
            }
            if(!_listeners[symbol][interval].ContainsKey(subscriber))
            {
                _listeners[symbol][interval].Add(subscriber, 1);
            } else
            {
                _listeners[symbol][interval][subscriber]++;
            }
            if(_listeners[symbol][interval].Count == 1 && _listeners[symbol][interval][subscriber] == 1)
            {
                return true;
            }
            return false;
        }

        public void AddListener(string[] symbols, KlineInterval interval, Subscriber subscriber)
        {
            var subscriptionSymbols = new List<string>();
            foreach (var symbol in symbols)
            {
                Console.WriteLine("Adding Listener on {0} - {1}", symbol, interval);
                if(AddListener(symbol, interval, subscriber))
                {
                    subscriptionSymbols.Add(symbol);
                }
            }
            _ = SubscribeSymbol(interval, subscriptionSymbols.ToArray());
        }

        public void RemoveListener(string[] symbols, KlineInterval interval, Subscriber subscriber)
        {
            var unsubscribeSymbols = new List<string>();
            foreach(var symbol in symbols)
            {
                if(RemoveListener(symbol, interval, subscriber))
                {
                    unsubscribeSymbols.Add(symbol);
                }
            }
            _ = UnsubscribeSymbol(interval, unsubscribeSymbols.ToArray());
        }

        private bool RemoveListener(string symbol, KlineInterval interval, Subscriber subscriber)
        {
            if (_listeners.ContainsKey(symbol) && _listeners[symbol].ContainsKey(interval) && _listeners[symbol][interval].ContainsKey(subscriber))
            {
                _listeners[symbol][interval][subscriber]--;
                if (_listeners[symbol][interval][subscriber] <= 0)
                {
                    _listeners[symbol][interval].Remove(subscriber);

                    if (_listeners[symbol][interval].Count == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual void Publish()
        {
            if(_candles == null || _candles.Count == 0)
            {
                return;
            }
            var _dirtyCandles = new Dictionary<KlineInterval, Dictionary<string, Kline>>();
            foreach(var interval in _candles.Keys)
            {
                foreach(var symbol in _candles[interval].Keys)
                {
                    if (_candles[interval][symbol].Value)
                    {
                        Console.WriteLine("{0}: {1} - {2} is Dirty", _exchange, symbol, interval);
                        if(!_dirtyCandles.ContainsKey(interval))
                        {
                            _dirtyCandles.Add(interval, new Dictionary<string, Kline>());
                        }
                        if(!_dirtyCandles[interval].ContainsKey(symbol))
                        {
                            _dirtyCandles[interval].Add(symbol, _candles[interval][symbol].Key);
                        }
                    }
                }
            }
        }

        public void Unsubscribe(Subscriber subscriber)
        {
            var unsubscriptionSymbols = new Dictionary<KlineInterval, List<string>>();
            foreach(var symbolLayer in _listeners)
            {
                foreach(var intervalLayer in _listeners[symbolLayer.Key])
                {
                    if(_listeners[symbolLayer.Key][intervalLayer.Key].ContainsKey(subscriber))
                    {
                        _listeners[symbolLayer.Key][intervalLayer.Key].Remove(subscriber);
                        if(_listeners[symbolLayer.Key][intervalLayer.Key].Count == 0)
                        {
                            if(!unsubscriptionSymbols.ContainsKey(intervalLayer.Key))
                            {
                                unsubscriptionSymbols.Add(intervalLayer.Key, new List<string>());
                            }
                            unsubscriptionSymbols[intervalLayer.Key].Add(symbolLayer.Key);
                        }
                    }
                }
            }
            foreach(var item in unsubscriptionSymbols)
            {
                _ = UnsubscribeSymbol(item.Key, item.Value.ToArray());
            }
        }

        public abstract Task SubscribeSymbol(KlineInterval interval, params string[] symbols);

        public abstract Task UnsubscribeSymbol(KlineInterval interval, params string[] symbols);
    }
}
