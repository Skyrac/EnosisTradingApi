using Binance.Net.Enums;
using CandleService.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> _historyCandles = new Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>>();
        private Dictionary<KlineInterval, Dictionary<string, int>> _requiredCandles = new Dictionary<KlineInterval, Dictionary<string, int>>();
        private Timer _timer;
        public BaseExchange(EExchange exchange)
        {
            _exchange = exchange;
            _timer = new Timer((_) => Publish(), null, 0, 1000);
        }

        protected abstract string[] GetSymbols();
        public async Task AddListener(WrappedIntervalCandles[] items, Subscriber subscriber, int requiredCandles)
        {
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
            }
            var subscriptionSymbols = new List<string>();
            foreach (var intervalItem in items)
            {
                if (!_requiredCandles.ContainsKey(intervalItem.Interval))
                {
                    _requiredCandles.Add(intervalItem.Interval, new Dictionary<string, int>(intervalItem.Candles.Count));
                }
                foreach (var symbolItem in intervalItem.Candles)
                {
                    if (!_requiredCandles[intervalItem.Interval].ContainsKey(symbolItem.Symbol))
                    {
                        _requiredCandles[intervalItem.Interval].Add(symbolItem.Symbol, requiredCandles);
                    }
                    else
                    {
                        _requiredCandles[intervalItem.Interval][symbolItem.Symbol] = Math.Max(requiredCandles, _requiredCandles[intervalItem.Interval][symbolItem.Symbol]);
                    }
                    Console.WriteLine("Adding Listener on {0} - {1}", symbolItem.Symbol, intervalItem.Interval);
                    if (AddListener(symbolItem.Symbol, intervalItem.Interval, subscriber))
                    {
                        subscriptionSymbols.Add(symbolItem.Symbol);
                    }
                }
                await SubscribeSymbol(intervalItem.Interval, subscriptionSymbols.ToArray());
            }
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

        protected bool CheckUp = false;

        public virtual void Publish()
        {
            if (_candles == null || _candles.Count == 0 || _subscribers.Count == 0)
            {
                return;
            }
            if(CheckUp)
            {
                Console.WriteLine("CheckUp Reached in Publish!");
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
                        var kline = _candles[interval][symbol];
                        kline.Dirty = false;
                        AddOrUpdateHistoryCandle(interval, symbol, kline, true);
                        if (!_listeners.ContainsKey(symbol) || !_listeners[symbol].ContainsKey(interval))
                        {
                            continue;
                        }
                        _wrappedIntervals[index].Candles.Add(new WrappedSymbolCandle(symbol, new Kline(kline)));
                        items++;
                    }
                }

                index++;
            }
            if (_wrappedIntervals == null || _wrappedIntervals.Count == 0 || _subscribers.Count == 0)
            {
                return;
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

        public virtual async Task<KeyValuePair<KlineInterval, KeyValuePair<string, List<Kline>>>> GetKlinesAsync(string symbol, KlineInterval interval, int? candles, DateTime? start, DateTime? end)
        {
            var minutes = ConvertInterval(interval);
            var items = new KeyValuePair<KlineInterval, KeyValuePair<string, List<Kline>>>(interval, new KeyValuePair<string, List<Kline>>(symbol, new List<Kline>()));
            if (start == null && end == null && candles != null && _historyCandles.ContainsKey(interval) && _historyCandles[interval].ContainsKey(symbol) && _historyCandles[interval][symbol].Values.Count >= candles)
            {
                items.Value.Value.AddRange(_historyCandles[interval][symbol].Values);
                return items;
            }
            var counter = 0;

            if (start != null)
            {
                var difference = end - start;
                counter = (int)Math.Ceiling(difference.Value.TotalMinutes / (int)minutes / 500);
            }
            while (counter >= 0)
            {
                if (start != null && start >= end)
                {
                    break;
                }
                var klines = await RequestKlines(symbol, interval, candles, start, end);
                items.Value.Value.AddRange(klines);
                if (start != null)
                {
                    start = start.Value.AddMinutes(klines.Count() * (int)minutes);
                }
                counter--;
            }
            AddOrUpdateHistoryCandles(interval, symbol, items.Value.Value);
            return items;
        }

        protected void AddOrUpdateHistoryCandles(KlineInterval interval, string symbol, IEnumerable<Kline> klines)
        {
            foreach(var kline in klines)
            {
                AddOrUpdateHistoryCandle(interval, symbol, kline);
            }
        }

        private void AddOrUpdateHistoryCandle(KlineInterval interval, string symbol, Kline kline, bool order = false)
        {
            if(_historyCandles == null) { return; }
            if (!_historyCandles.ContainsKey(interval))
            {
                _historyCandles.Add(interval, new Dictionary<string, Dictionary<DateTime, Kline>>());
            }
            if (!_historyCandles[interval].ContainsKey(symbol))
            {
                _historyCandles[interval].Add(symbol, new Dictionary<DateTime, Kline>());
            }
            if (!_historyCandles[interval][symbol].ContainsKey(kline.Date))
            {
                Console.WriteLine("{3}: Added new Candle to {0} - {1} (Summe: {2})", interval, symbol, _historyCandles[interval][symbol].Count, DateTime.Now);
                _historyCandles[interval][symbol].Add(kline.Date, kline);
            }
            else
            {
                _historyCandles[interval][symbol][kline.Date].Update(kline);
            }
            OrderCandles(interval, symbol);
            RemoveUnnecessaryCandles(interval, symbol);
        }

        private void OrderCandles(KlineInterval interval, string symbol)
        {
            _historyCandles[interval][symbol] = _historyCandles[interval][symbol].OrderBy(item => item.Key).ToDictionary(keyItem => keyItem.Key, valItem => valItem.Value);
        }

        private void RemoveUnnecessaryCandles(KlineInterval interval, string symbol)
        {
            while(_historyCandles[interval][symbol].Keys.Count > _requiredCandles[interval][symbol])
            {
                if(_historyCandles[interval][symbol].Remove(_historyCandles[interval][symbol].First().Key, out Kline kline))
                {
                    Console.WriteLine("{4}: Removed Candle from {0} - {1} with start {2}! Remaining: {3}", symbol, interval, kline.Date, _historyCandles[interval][symbol].Keys.Count, DateTime.Now);
                }
            }
        }

        protected abstract Task<IEnumerable<Kline>> RequestKlines(string symbol, KlineInterval interval, int? candles, DateTime? start, DateTime? end);

        private int ConvertInterval(KlineInterval interval)
        {
            switch (interval)
            {
                case KlineInterval.OneMinute:
                    return 1;
                case KlineInterval.ThreeMinutes:
                    return 3;
                case KlineInterval.FiveMinutes:
                    return 5;
                case KlineInterval.FifteenMinutes:
                    return 15;
                case KlineInterval.ThirtyMinutes:
                    return 30;
                case KlineInterval.OneHour:
                    return 60;
                case KlineInterval.TwoHour:
                    return 120;
                case KlineInterval.FourHour:
                    return 240;
                case KlineInterval.SixHour:
                    return 360;
                case KlineInterval.EightHour:
                    return 480;
                case KlineInterval.TwelveHour:
                    return 720;
                case KlineInterval.OneDay:
                    return 1440;
                case KlineInterval.ThreeDay:
                    return 1440 * 3;
                case KlineInterval.OneWeek:
                    return 1440 * 7;
                case KlineInterval.OneMonth:
                    return 1440 * 30;
                default: return 1;
            }
        }

        public abstract Task SubscribeSymbol(KlineInterval interval, params string[] symbols);

        public abstract Task UnsubscribeSymbol(KlineInterval interval, params string[] symbols);
    }
}
