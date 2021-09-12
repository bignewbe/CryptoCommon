using PortableCSharpLib.DataType;
using System;
using System.Collections.Generic;

namespace CryptoCommon.DataTypes
{
    public class Orderbook 
    {
        public long Timestamp { get; set; }
        public string Symbol { get; set; }
        public List<PriceVolumePair> Asks { get; set; }
        public List<PriceVolumePair> Bids { get; set; }
    }
}
