using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Candles.Models;
using Utils.Trading.Enums;

namespace Utils.Strategies.Implemented
{
    public class EmaCrossStrategy : BaseEntryStrategy
    {

        public decimal PriceMovementMultiplier = new decimal(3);
        public override decimal CheckEntry(ESide side, IEnumerable<Kline> klines, int precision, int index = -1)
        {
            index = index > 0 ? index : klines.Count() - 1;
            if (index >= klines.Count())
                return -1;
            var candle = klines.ElementAt(index);
            var previousCandle = klines.ElementAt(index - 1);

            var adxCandle = candle.GetIndicator<AdxResult>("ADX");
            var atr = (decimal)candle.GetIndicator<AtrResult>("ATR").Atr;
            var volSma = candle.GetIndicator<VolSmaResult>("VOL_SMA").VolSma;
            bool canEnter = CheckEntry(candle, side) && !CheckEntry(previousCandle, side) && volSma < candle.Volume && adxCandle.Adx > 25 && adxCandle.Mdi < adxCandle.Pdi;
            if(canEnter && side == ESide.Long)
            {
                return candle.Close - atr * PriceMovementMultiplier;
            }
            if(canEnter && side == ESide.Short)
            {
                return  candle.Close - atr * PriceMovementMultiplier;
            }
            return -1;

        }

        private bool CheckEntry(Kline candle, ESide side)
        {
            var ema_8 = candle.GetIndicator<EmaResult>("EMA_8").Ema;
            var ema_21 = candle.GetIndicator<EmaResult>("EMA_21").Ema;
            var ema_34 = candle.GetIndicator<EmaResult>("EMA_34").Ema;
            if(side == ESide.Long)
            {
                return ema_21 < ema_8
                    && ema_34 < ema_21
                    && ema_34 < candle.Close;
            } else if(side == ESide.Short)
            {
                return ema_21 > ema_8
                    && ema_34 > ema_21
                    && ema_34 > candle.Close;
            }
            return false;
        }

        public void SetupIndicators(Dictionary<DateTime, Kline> dict)
        {
            if (dict.Count == 0)
            {
                Console.WriteLine("Tried to setup Indicators without values!");
                return;
            }
            var indicators = new Dictionary<string, IEnumerable<ResultBase>>();
            indicators.Add("EMA_34", Indicator.GetEma(dict.Values, 34));
            indicators.Add("EMA_8", Indicator.GetEma(dict.Values, 8));
            indicators.Add("EMA_21", Indicator.GetEma(dict.Values, 21));
            indicators.Add("VOL_SMA", Indicator.GetVolSma(dict.Values, 48));
            indicators.Add("ADX", Indicator.GetAdx(dict.Values, 9)); 
            indicators.Add("ATR", Indicator.GetAtr(dict.Values, 8));
            foreach (var indicator in indicators)
            {
                foreach (var item in indicator.Value)
                {
                    dict[item.Date].FillIndicator(indicator.Key, item);
                }
            }
        }
    }
}
