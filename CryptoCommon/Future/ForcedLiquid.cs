using CryptoCommon.DataTypes;
using System;

namespace CryptoCommon.Future.Interface
{
    public class ForcedLiquid
    {
        public double Loss { get { return  FZOrder.ConvertStrToDouble(loss); } }
        public double Size { get { return  FZOrder.ConvertStrToDouble(size); } }
        public double Price { get { return FZOrder.ConvertStrToDouble(price); } }

        public string loss { get; set; }
        public string size { get; set; }
        public string price { get; set; }
        public DateTime created_at { get; set; }
        public string type { get; set; }
        public string instrument_id { get; set; }
    }
}
