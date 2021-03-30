using System.Collections.Generic;
using Utils.Candles.Models;
using Utils.Trading.Enums;

namespace Utils.Strategies
{
    public abstract class BaseEntryStrategy : BaseStrategy
    {
        public abstract decimal CheckEntry(ESide side, IEnumerable<Kline> klines, int precision, int index = -1);
    }
}
