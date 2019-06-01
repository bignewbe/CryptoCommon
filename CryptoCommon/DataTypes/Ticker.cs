using PortableCSharpLib;

namespace CryptoCommon.DataTypes
{
    public class Ticker
    {
        public string TimeStr { get { return Seconds.GetUTCFromUnixTime().ToLocalTime().ToString("yyyyMMdd hh:mm:ss"); } }
        public string Symbol { get; set; }
        public int Miliseconds { get { return (int)(Timestamp % 1000); } }
        public long Seconds { get { return Timestamp / 1000; } }
        public int OpenBuyOrders { get; set; }
        public int OpenSellOrders { get; set; }
        public long Timestamp { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Last { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
        public double BaseVolume { get; set; }

        public Ticker()
        {
        }
        public Ticker(Ticker other)
        {
            this.Copy(other);
        }

        public void Copy(Ticker other)
        {
            this.Symbol = other.Symbol;
            this.Timestamp = other.Timestamp;
            this.Bid = other.Bid;
            this.Ask  = other.Ask;
            this.Last = other.Last;
            this.High = other.High;
            this.Low = other.Low;
            this.BaseVolume = other.BaseVolume;
            this.Volume = other.Volume;
            this.OpenBuyOrders = other.OpenBuyOrders;
            this.OpenSellOrders = other.OpenSellOrders;
        }
    }
}
