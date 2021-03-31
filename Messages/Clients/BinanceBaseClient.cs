using Binance.Net;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;

namespace Utils.Clients
{
    public class BinanceBaseClient : BaseClient
    {
        public BinanceClient Client { get; set; }
        public BinanceBaseClient(string key = "vBEZXTHGHLprYlxAvplvkEdIKJmc7pECA5CNO6pgWLQnxyMPOOoZ0Ccz2rWn6irn", string secret = "cDgmpQiRZxNHdvFVG7mbDxSytIO6MPogZOKFa90rIwNpdFBHZdhuBtnY04G2X8YH")
        {
            Client = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials(key, secret)
            });
        }
    }
}
