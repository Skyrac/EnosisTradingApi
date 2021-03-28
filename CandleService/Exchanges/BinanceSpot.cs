using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CandleService.Exchanges
{
    public class BinanceSpot : IExchange
    {
        private readonly string _key = "vBEZXTHGHLprYlxAvplvkEdIKJmc7pECA5CNO6pgWLQnxyMPOOoZ0Ccz2rWn6irn";
        private readonly string _secret = "cDgmpQiRZxNHdvFVG7mbDxSytIO6MPogZOKFa90rIwNpdFBHZdhuBtnY04G2X8YH";
        private BinanceClient client;
        private BinanceSocketClient socketClient;
        private bool finishedSetup = false;

        private Dictionary<string, object> _candles = new Dictionary<string, object>();
        public BinanceSpot()
        {
            client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(_key, _secret)
            });
            WebCallResult<string> stream;
            do
            {
                stream = client.Spot.UserStream.StartUserStream();
                if (!stream.Success)
                {
                    Console.WriteLine("ERROR WHILE INITIATING BINANCE FUTURES CLIENT USER STREAM: {0}", stream.Error);
                    Thread.Sleep(5000);
                }
            } while (!stream.Success);
            socketClient = new BinanceSocketClient();
            _ = SubscribeSymbols();
        }

        private async Task SubscribeSymbols()
        {
            var symbols = GetSymbols();
            Console.WriteLine("FOUND {0} Symbols on Binance Spot.", symbols.Length);
            var watch = new Stopwatch();
            watch.Start();
            var tasks = new List<Task>();
            for(var i = 0; i < symbols.Length; i++)
            {
                tasks.Add(SubscribeSymbol(symbols[i]));
                if(i % 50 == 0)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }
            if (tasks.Count != 0)
            {
                await Task.WhenAll(tasks);
            }
            Console.WriteLine("Needed {0}s to start", watch.ElapsedMilliseconds / 1000);
            watch.Stop();
        }

        private async Task SubscribeSymbol(string symbol)
        {
            var success = false;
            do
            {
                var subscription = await socketClient.Spot.SubscribeToKlineUpdatesAsync(symbol, Binance.Net.Enums.KlineInterval.OneMinute, RecieveCandleUpdate);
                success = subscription.Success;
                if (!success)
                {
                    Console.WriteLine("ERROR SUBSCRIBING TO {0}: {1}", symbol, subscription.Error);
                }

            } while (!success);
            counter++;
            Console.WriteLine("Subscribed {0}", symbol);
            if (counter >= 1366)
            {
                Console.WriteLine("Alle subscribed :)");
                counter = 0;
                finishedSetup = true;
            }
        }

        int counter = 0;
        private void RecieveCandleUpdate(IBinanceStreamKlineData obj)
        {
            if(!_candles.ContainsKey(obj.Symbol))
            {
                _candles.Add(obj.Symbol, obj.Data);
            }
            if (obj.Data.Final && finishedSetup)
            {
                counter++;
                if(counter == 1366)
                {
                    counter = 0;
                    Console.WriteLine("All Updated at {0}", DateTime.Now);
                }
            }
        }

        public string[] GetSymbols()
        {
            var request = client.Spot.Market.GetAllBookPrices();
            var symbols = new List<string>();
            while (!request.Success)
            {
                Thread.Sleep(2000);
                request = client.Spot.Market.GetAllBookPrices();
            }
            var data = request.Data;
            foreach (var price in data)
            {
                if(!price.Symbol.Contains("UP") && !price.Symbol.Contains("DOWN") && !symbols.Contains("BEAR") && !symbols.Contains("BULL") && (price.Symbol.Contains("usdt", StringComparison.OrdinalIgnoreCase) || price.Symbol.Contains("btc", StringComparison.OrdinalIgnoreCase) || price.Symbol.Contains("eth", StringComparison.OrdinalIgnoreCase)))
                    symbols.Add(price.Symbol);
            }
            return symbols.ToArray();
        }
    }
}
