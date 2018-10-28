using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public class PriceVolumePair
    {
        public double Price { get; set; }
        public double Volume { get; set; }
    }
    public class Orderbook
    {
        public List<PriceVolumePair> Asks { get; set; }
        public List<PriceVolumePair> Bids { get; set; }
    }
}
