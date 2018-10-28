using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public class Order:IEquatable<Order>
    {
        public DateTime LocalTime { get { return CreateDate.ToLocalTime(); } }
        public string Exchange { get; set; }
        public DateTime CreateDate { get; set; }
        public string Symbol { get; set; }
        public double Amount { get; set; }
        public double AvgPrice { get; set; }
        public double DealAmount { get; set; }
        public string OrderId { get; set; }
        public double Price { get; set; }
        public double CommissionPaid { get; set; }
        public OrderStatus Status { get; set; }
        public OrderType Ordertype { get; set; }
        public string RefId { get; set; }

        public Order()
        {
        }

        public Order(Order other)
        {
            this.Copy(other);
        }

        public bool Equals(Order other)
        {
            return (this.Exchange == other.Exchange &&
                this.CreateDate == other.CreateDate &&
                this.Symbol == other.Symbol &&
                this.Amount == other.Amount &&
                this.AvgPrice == other.AvgPrice &&
                this.DealAmount == other.DealAmount &&
                this.OrderId == other.OrderId &&
                this.Price == other.Price &&
                this.Status == other.Status &&
                this.Ordertype == other.Ordertype &&
                this.RefId == other.RefId &&
                this.CommissionPaid == other.CommissionPaid);
        }

        public void Copy(Order other)
        {
            this.Exchange = other.Exchange;
            this.CreateDate = other.CreateDate;
            this.Symbol = other.Symbol;
            this.Amount = other.Amount;
            this.AvgPrice = other.AvgPrice;
            this.DealAmount = other.DealAmount;
            this.OrderId = other.OrderId;
            this.Price = other.Price;
            this.Status = other.Status;
            this.Ordertype = other.Ordertype;
            this.RefId = other.RefId;
            this.CommissionPaid = other.CommissionPaid;
        }
    }
}
