using PortableCSharpLib.Interace;
using System;

namespace CryptoCommon.Future.Interface
{
    public class PublicTrade : IIdEqualCopy<PublicTrade>
    {
        public double Price { get { return FutureOrder.ConvertStrToDouble(price); } }
        public double Qty { get { return FutureOrder.ConvertStrToDouble(qty); } }

        public string trade_id { get; set; }
        public string side { get; set; }
        public string price { get; set; }
        public string qty { get; set; }
        public DateTime timestamp { get; set; }

        public string Id { get { return trade_id; } }

        public void Copy(PublicTrade other)
        {
            this.trade_id = other.trade_id;
            this.side = other.side;
            this.price = other.price;
            this.qty = other.qty;
            this.timestamp = other.timestamp;
        }

        public bool Equals(PublicTrade other)
        {
            return (
            this.trade_id == other.trade_id &&
            this.side == other.side &&
            this.price == other.price &&
            this.qty == other.qty &&
            this.timestamp == other.timestamp);
        }
    }
}
