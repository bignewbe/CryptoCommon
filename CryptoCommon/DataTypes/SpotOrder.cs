using PortableCSharpLib.Interace;
using System;

namespace CryptoCommon.DataTypes
{
    public class SpotOrder:  IIdEqualCopy<SpotOrder>
    {
        public DateTime LocalTime { get { return TimeCreated.ToLocalTime(); } }
        public string Id { get { return OrderId; } }
        public string Exchange { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeLast { get; set; }
        public string Symbol { get; set; }
        public double Amount { get; set; }
        public double AvgPrice { get; set; }
        public double DealAmount { get; set; }
        public string OrderId { get; set; }
        public double Price { get; set; }
        public double CommissionPaid { get; set; }
        public OrderState State { get; set; }
        public OrderType Ordertype { get; set; }
        public string PrevRefId { get; set; }
        public string RefId { get; set; }

        public SpotOrder()
        {
        }

        public SpotOrder(SpotOrder other)
        {
            this.Copy(other);
        }

        public bool Equals(SpotOrder other)
        {
            if (other == null) return false;

            return (this.Exchange == other.Exchange &&
                this.TimeCreated == other.TimeCreated &&
                this.TimeLast == other.TimeLast &&
                this.Symbol == other.Symbol &&
                this.Amount == other.Amount &&
                this.AvgPrice == other.AvgPrice &&
                this.DealAmount == other.DealAmount &&
                this.OrderId == other.OrderId &&
                this.Price == other.Price &&
                this.State == other.State &&
                this.Ordertype == other.Ordertype &&
                this.RefId == other.RefId &&
                this.CommissionPaid == other.CommissionPaid);
        }

        public void Copy(SpotOrder other)
        {
            if (other != null)
            {
                this.Exchange = other.Exchange;
                this.TimeCreated = other.TimeCreated;
                this.TimeLast = other.TimeLast;
                this.Symbol = other.Symbol;
                this.Amount = other.Amount;
                this.AvgPrice = other.AvgPrice;
                this.DealAmount = other.DealAmount;
                this.OrderId = other.OrderId;
                this.Price = other.Price;
                this.State = other.State;
                this.Ordertype = other.Ordertype;
                this.RefId = other.RefId;
                this.CommissionPaid = other.CommissionPaid;
            }
        }
    }
}
