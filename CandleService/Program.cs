using CandleService.Exchanges;
using System;

namespace CandleService
{
    public class Program
    {
        static void Main(string[] args)
        {
            new BinanceSpot();
            Console.ReadKey();
        }
    }
}
