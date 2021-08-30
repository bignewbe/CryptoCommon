using CryptoCommon.DataTypes;
using System;

namespace CryptoCommon.Future.Interface
{
    public class BidAskPrice
    {
        public double Bid { get { return FZOrder.ConvertStrToDouble(lowest); } }
        public double Ask { get { return FZOrder.ConvertStrToDouble(highest); } }
        public string instrument_id { get; set; }
        public string highest { get; set; }
        public string lowest { get; set; }
        public DateTime timestamp { get; set; }
    }
}
