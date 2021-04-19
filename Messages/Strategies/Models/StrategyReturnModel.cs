using Binance.Net.Enums;

namespace Utils.Strategies.Models
{
    public class StrategyReturnModel
    {
        public string CoreSymbol { get; set; }
        public KlineInterval CoreInterval {get;set;}
        public bool IsTrue { get; set; }
        public decimal Value { get; set; }
        public static StrategyReturnModel False => new StrategyReturnModel() { IsTrue = false };
        public static StrategyReturnModel True => new StrategyReturnModel() { IsTrue = true };
    }
}
