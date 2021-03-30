using System;

namespace Utils.Trading
{
    public class TradeInfo
    {
        public string Symbol { get; set; }
        public decimal Entry { get; set; }
        public decimal High { get; set; }
        public string Side { get; set; }
        public decimal Low { get; set; }
        public decimal Stop { get; set; }
        private decimal stopLoss;
        public decimal StopLoss
        {
            get
            {
                return stopLoss;
            }
            set
            {
                Stop = value;
                stopLoss = value;
            }
        }
        public decimal Profit { get; set; }
        public decimal Balance { get; set; }
        public decimal RawBalance { get; set; }
        public bool Close { get; set; }
        public DateTime Opened { get; set; }
        public DateTime Closed { get; set; }
        public decimal StopPercentage { get; internal set; }
        public bool Finished { get; set; } = false;
        public decimal EstimatedEntry { get; internal set; }
    }
}
