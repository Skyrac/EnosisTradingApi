using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot;
using CandleService.Services;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Messages.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace CandleService.Exchanges
{
    public class BinanceExchange : BaseExchange
    {
        private readonly string _key = "vBEZXTHGHLprYlxAvplvkEdIKJmc7pECA5CNO6pgWLQnxyMPOOoZ0Ccz2rWn6irn";
        private readonly string _secret = "cDgmpQiRZxNHdvFVG7mbDxSytIO6MPogZOKFa90rIwNpdFBHZdhuBtnY04G2X8YH";
        private BinanceClient client;
        private BinanceSocketClient socketClient;
        private bool finishedSetup = false;

        private Dictionary<string, object> _candles = new Dictionary<string, object>();
        public BinanceExchange(EExchange exchangeType) : base(exchangeType)
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
        }

        public override async Task SubscribeSymbol(KlineInterval interval, params string[] symbols)
        {
            if (symbols == null)
            {
                symbols = GetSymbols();
            }
            var watch = new Stopwatch();
            watch.Start();
            var tasks = new List<Task>();
            for (var i = 0; i < symbols.Length; i++)
            {
                tasks.Add(SubscribeSymbol(symbols[i], interval));
                if (i % 50 == 0)
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
            finishedSetup = true;
            watch.Stop();
        }

        private async Task SubscribeSymbol(string symbol, KlineInterval interval)
        {
            var success = false;
            do
            {
                CallResult<CryptoExchange.Net.Sockets.UpdateSubscription> subscription = null;
                if(_exchange == EExchange.BinanceSpot)
                {
                    subscription = await socketClient.Spot.SubscribeToKlineUpdatesAsync(symbol, interval, (update) => {

                        RecieveCandleUpdate(interval, update);
                    });
                }
                else if (_exchange == EExchange.BinanceFuturesUsdt)
                {
                    subscription = await socketClient.FuturesUsdt.SubscribeToKlineUpdatesAsync(symbol, interval, (update) => {

                        RecieveCandleUpdate(interval, update);
                    });
                }
                success = subscription.Success;
                if (!success)
                {
                    Console.WriteLine("ERROR SUBSCRIBING TO {0}: {1}", symbol, subscription.Error);
                }

            } while (!success);
            counter++;
            Console.WriteLine("Subscribed {0}", symbol);
        }

        int counter = 0;
        private void RecieveCandleUpdate(KlineInterval interval, IBinanceStreamKlineData obj)
        {
            if(!_candles.ContainsKey(obj.Symbol))
            {
                _candles.Add(obj.Symbol, obj.Data);
            }
            if (obj.Data.Final && finishedSetup)
            {
                Console.WriteLine("{0}: RECIEVED UPDATE ON {1} AT {2}", _exchange, obj.Symbol, DateTime.Now);
            }
        }

        public string[] GetSymbols()
        {

            WebCallResult<IEnumerable<Binance.Net.Objects.Spot.MarketData.BinanceBookPrice>> request = null;
            if(_exchange == EExchange.BinanceSpot)
            {
                request = client.Spot.Market.GetAllBookPrices();
            }
            else if (_exchange == EExchange.BinanceFuturesUsdt)
            {
                request = client.FuturesUsdt.Market.GetBookPrices();
            }
            var symbols = new List<string>();
            while (!request.Success)
            {
                Thread.Sleep(2000);
                if (_exchange == EExchange.BinanceSpot)
                {
                    request = client.Spot.Market.GetAllBookPrices();
                } else if(_exchange == EExchange.BinanceFuturesUsdt)
                {
                    request = client.FuturesUsdt.Market.GetBookPrices();
                }
            }
            var data = request.Data;
            foreach (var price in data)
            {
                if(!price.Symbol.Contains("UP") && !price.Symbol.Contains("DOWN") && !price.Symbol.Contains("BEAR") && !price.Symbol.Contains("BULL") && (price.Symbol.EndsWith("usdt", StringComparison.OrdinalIgnoreCase) || price.Symbol.EndsWith("btc", StringComparison.OrdinalIgnoreCase) || price.Symbol.EndsWith("eth", StringComparison.OrdinalIgnoreCase)))
                    symbols.Add(price.Symbol);
            }
            return symbols.ToArray();
        }

        public override async Task UnsubscribeSymbol(KlineInterval interval, params string[] symbols)
        {
            throw new NotImplementedException();
        }
    }
}
