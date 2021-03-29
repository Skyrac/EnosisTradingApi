using Binance.Net.Interfaces;
using Newtonsoft.Json;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;

namespace Utils.Candles.Models
{
    public class Kline : Quote
    {
        public DateTime CloseTime
        {
            get; set;
        }

        [JsonIgnore]
        public bool Dirty { get; set; } = false;
        //public Dictionary<string, ResultBase> Indicators { get; set; } = new Dictionary<string, ResultBase>();

        public Kline() { }

        public Kline(IBinanceKline kline)
        {
            this.Date = kline.OpenTime;
            this.Open = kline.Open;
            this.Close = kline.Close;
            this.High = kline.High;
            this.Low = kline.Low;
            this.Volume = kline.BaseVolume;
            this.CloseTime = kline.CloseTime;
        }

        public Kline(IBinanceStreamKline kline)
        {
            this.Date = kline.OpenTime;
            this.Open = kline.Open;
            this.Close = kline.Close;
            this.High = kline.High;
            this.Low = kline.Low;
            this.Volume = kline.BaseVolume;
            this.CloseTime = kline.CloseTime;
        }

        public Kline(Kline kline)
        {
            this.Date = kline.Date;
            this.Open = kline.Open;
            this.Close = kline.Close;
            this.High = kline.High;
            this.Low = kline.Low;
            this.Volume = kline.Volume;
            this.CloseTime = kline.CloseTime;
        }
        //public T GetIndicator<T>(string indicator) where T : ResultBase
        //{
        //    if (Indicators.ContainsKey(indicator) && Indicators[indicator].GetType() == typeof(T))
        //    {
        //        return (T)Indicators[indicator];
        //    }

        //    return null;
        //}

        public void Update(IBinanceKline kline)
        {
            this.Date = kline.OpenTime;
            this.Open = kline.Open;
            this.Close = kline.Close;
            this.High = kline.High;
            this.Low = kline.Low;
            this.Volume = kline.BaseVolume;
        }
        public void Update(Kline kline)
        {
            this.Date = kline.Date;
            this.Open = kline.Open;
            this.Close = kline.Close;
            this.High = kline.High;
            this.Low = kline.Low;
            this.Volume = kline.Volume;
        }


        public override string ToString()
        {
            return string.Format("Open Time: {0} | Open: {1} | Close: {2} | High: {3} | Low: {4}", Date, Open, Close, High, Low);
        }
    }
}
