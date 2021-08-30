using CryptoCommon.DataTypes;
using PortableCSharpLib.Interace;
using System;

namespace CryptoCommon.Future.Interface
{
    public class PublicHoldAmount : IIdEqualCopy<PublicHoldAmount>
    {
        public double Amount { get { return FZOrder.ConvertStrToDouble(amount); } }
        public string instrument_id { get; set; }
        public string amount { get; set; }
        public DateTime timestamp { get; set; }

        public string Id { get { return instrument_id; } }

        public void Copy(PublicHoldAmount other)
        {
            this.instrument_id = other.instrument_id;
            this.amount = other.amount;
            this.timestamp = other.timestamp;
        }
        public bool Equals(PublicHoldAmount other)
        {
            return (
                this.instrument_id == other.instrument_id &&
                this.amount == other.amount &&
                this.timestamp == other.timestamp);
        }
    }
}
