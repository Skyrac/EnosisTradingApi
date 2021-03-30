using Binance.Net.Enums;
using CandleService.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Utils.Candles.Models;
using Utils.Enums;
using Utils.Messages.Models;

namespace CandleService.Exchanges
{
    public abstract class BaseExchange
    {
        protected EExchange _exchange;

        protected Dictionary<string, Dictionary<KlineInterval, Dictionary<Subscriber, int>>> _listeners = new Dictionary<string, Dictionary<KlineInterval, Dictionary<Subscriber, int>>>();
        protected ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> _candles = new ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>>();
        private List<Subscriber> _subscribers = new List<Subscriber>();
        private Timer _timer;
        public BaseExchange(EExchange exchange)
        {
            _exchange = exchange;
            _timer = new Timer((_) => Publish(), null, 0, 1000);
        }

        protected abstract string[] GetSymbols();

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
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
            var subscriptionSymbols = new List<string>();
            if(symbols == null || symbols.Length == 0)
            {
                symbols = GetSymbols();
            }
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
            if(_candles == null || _candles.Count == 0 || _subscribers.Count == 0)
            {
                return;
            }
            var _wrappedIntervals = new List<WrappedIntervalCandles>();
            var availableCandles = new ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>>(_candles);
            var watch = new Stopwatch();
            watch.Start();
            var index = 0;
            var items = 0;
            foreach(var interval in availableCandles.Keys)
            {
                _wrappedIntervals.Add(new WrappedIntervalCandles(interval));
                foreach (var symbol in availableCandles[interval].Keys)
                {
                    if (availableCandles[interval] != null && availableCandles[interval].ContainsKey(symbol) && availableCandles[interval][symbol].Dirty)
                    {
                        if(!_listeners.ContainsKey(symbol) || !_listeners[symbol].ContainsKey(interval))
                        {
                            continue;
                        }
                        var kline = _candles[interval][symbol];
                        kline.Dirty = false;
                        _wrappedIntervals[index].Candles.Add(new WrappedSymbolCandle(symbol, new Kline(kline)));
                        items++;
                    }
                }

                index++;
            }
            var message = JsonConvert.SerializeObject(new CandleServiceUpdateMessage(_exchange, _wrappedIntervals));
            foreach(var subscriber in _subscribers)
            {
                subscriber.Context.WebSocket.Send(message);
            }
            Console.WriteLine("{0}: Needed {1} seconds to publish {2} dirty candles", _exchange, watch.ElapsedMilliseconds / 1000, items);
            watch.Stop();
        }

        public void Unsubscribe(Subscriber subscriber)
        {
            if (_subscribers.Contains(subscriber))
            {
                _subscribers.Remove(subscriber);
            }
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
