using Binance.Net.Enums;
using System.Collections.Concurrent;
using Utils.Candles.Models;
using Utils.Strategies.Models;
using Utils.Trading;
using Utils.Trading.Enums;

namespace Utils.Strategies
{
    public class BaseStrategy
    {
        public ConditionSequence LongCondition { get; set; }
        public ConditionSequence ShortCondition { get; set; }

        public void FillIndicators()
        {
            
        }

        public virtual CloseReason CloseLong(TradeInfo info, ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles, int index = -1)
        {
            //var candle = index > 0 && index < klines.Count() ? klines.ElementAt(index) : klines.Last();
            //info.High = Math.Max(candle.High, info.High);
            //info.Low = Math.Min(candle.Low, info.Low);
            //if (candle.Low <= info.StopLoss)
            //{
            //    return CloseReason.StopLoss;
            //} else if(candle.High >= info.Stop)
            //{
            //    return CloseReason.TakeProfit;
            //}
            return CloseReason.NoClose;
        }

        public virtual CloseReason CloseShort(TradeInfo info, ConcurrentDictionary<KlineInterval, ConcurrentDictionary<string, Kline>> candles, int index = -1)
        {
            //var candle = index > 0 && index < klines.Count() ? klines.ElementAt(index) : klines.Last();
            //info.High = Math.Max(candle.High, info.High);
            //info.Low = Math.Min(candle.Low, info.Low);
            //if (candle.High >= info.StopLoss)
            //{
            //    return CloseReason.StopLoss;
            //}
            //else if (candle.Low <= info.Stop)
            //{
            //    return CloseReason.TakeProfit;
            //}
            return CloseReason.NoClose;
        }
    }
}
