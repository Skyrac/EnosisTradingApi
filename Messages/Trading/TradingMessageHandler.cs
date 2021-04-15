using Binance.Net.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Candles.Models;
using Utils.Messages.Models;
using Utils.Strategies;

namespace Utils.Trading
{
    public static class TradingMessageHandler
    {
        public static void HandleCandleServiceHistoryCandles(string rawData, ref Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, Dictionary<string, Strategy> strategies)
        {
            var historyCandles = JsonConvert.DeserializeObject<CandleServiceHistoryCandlesMessage>(rawData);
            if (historyCandles == null || historyCandles.Candles.Count == 0)
            {
                return;
            }
            foreach (var intervalCandle in historyCandles.Candles)
            {
                if (!candles.ContainsKey(intervalCandle.Interval))
                {
                    candles.Add(intervalCandle.Interval, new Dictionary<string, Dictionary<DateTime, Kline>>());
                }
                foreach (var symbolCandle in intervalCandle.Symbols)
                {
                    if (!candles[intervalCandle.Interval].ContainsKey(symbolCandle.Symbol))
                    {
                        candles[intervalCandle.Interval].Add(symbolCandle.Symbol, new Dictionary<DateTime, Kline>());
                    }
                    foreach (var kline in symbolCandle.Klines)
                    {
                        if (!candles[intervalCandle.Interval][symbolCandle.Symbol].ContainsKey(kline.Date))
                        {
                            candles[intervalCandle.Interval][symbolCandle.Symbol].Add(kline.Date, kline);
                        }
                    }
                    candles[intervalCandle.Interval][symbolCandle.Symbol] = candles[intervalCandle.Interval][symbolCandle.Symbol].OrderBy(item => item.Key).ToDictionary(keyItem => keyItem.Key, valueItem => valueItem.Value);
                }
            }
            if (strategies != null)
            {
                foreach (var strategy in strategies)
                {
                    strategy.Value.SetupIndicators(candles);
                }
            }
        }

        public static bool HandleCandleServiceUpdateAndCheckForNewCandle(string rawData, ref Dictionary<KlineInterval, Dictionary<string, Dictionary<DateTime, Kline>>> candles, Dictionary<string, Strategy> strategies)
        {
            var date = DateTime.Now;
            var data = JsonConvert.DeserializeObject<CandleServiceUpdateMessage>(rawData);
            if (data == null || data.IntervalCandles.Count == 0)
            {
                return false;
            }
            var newCandle = false;
            foreach (var intervalCandle in data.IntervalCandles)
            {
                if (!candles.ContainsKey(intervalCandle.Interval))
                {
                    candles.Add(intervalCandle.Interval, intervalCandle.ConvertCandlesToDictionary());
                }
                else
                {
                    foreach (var symbolCandle in intervalCandle.Candles)
                    {
                        if (!candles[intervalCandle.Interval].ContainsKey(symbolCandle.Symbol))
                        {
                            candles[intervalCandle.Interval].Add(symbolCandle.Symbol, new Dictionary<DateTime, Kline>() { { symbolCandle.Kline.Date, symbolCandle.Kline } });
                        }
                        else if (candles[intervalCandle.Interval][symbolCandle.Symbol].ContainsKey(symbolCandle.Kline.Date))
                        {
                            candles[intervalCandle.Interval][symbolCandle.Symbol][symbolCandle.Kline.Date].Update(symbolCandle.Kline);
                            if(symbolCandle.Kline.IsFinal)
                            {
                                Console.WriteLine("{0}: Recieved Final Candle on {1} - {2}", DateTime.Now,symbolCandle.Symbol, intervalCandle.Interval );
                            }
                        }
                        else
                        {
                            Console.WriteLine("Recieved new candle on {0} - {1} at {2}, Recieved Message at {3}", symbolCandle.Symbol, intervalCandle.Interval, DateTime.Now, date);
                            candles[intervalCandle.Interval][symbolCandle.Symbol].Add(symbolCandle.Kline.Date, symbolCandle.Kline);
                            newCandle = true;
                        }
                    }
                }
            }
            foreach (var strategy in strategies)
            {
                strategy.Value.SetupIndicators(candles);
            }
            return newCandle;
        }
    }
}
