using System;
using CryptoCommon.DataTypes;
using PortableCSharpLib.Interace;

namespace CryptoCommon.Future.Interface
{
    public class FutureOrder : SpotOrder,  IIdEqualCopy<FutureOrder> //: EqualAndCopyUseReflection<FutureOrder>
    {
        public static double ConvertStrToDouble(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return double.Parse(str);
        }

        public static int ConvertStrToInt(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return int.Parse(str);
        }

        //public string Exchange { get; set; }
        //public DateTime TimeCreated { get; set; }
        //public DateTime TimeLast { get; set; }
        //public double CommissionPaid { get; set; }   //fee
        //public double Price { get; set; }            
        //public double AvgPrice { get; set; }         
        //public string OrderId { get; set; }          
        //public string PrevRefId { get; set; }        //parent client_oid
        //public string RefId { get; set; }            //client_oid
        //public double DealAmount { get; set; }       //filled amount
        //public double Amount { get; set; }           //contract value, size
        //public string Symbol { get; set; }           //instrument_id
        //public OrderState State { get; set; }
        ////order_type String	0: Normal limit order 1: Post only 2: Fill Or Kill 3: Immediatel Or Cancel
        ////public string Ordertype { get; set; }
        //public OrderType Ordertype { get; set; }   //open long, open short, close long, clost short
        //public string Id { get { return OrderId; } }
        //public int Leverage { get; set; }

        public FutureOrder() : base()
        {
        }

        public FutureOrder(string exchange, string instrument_id, int amount, double price, OrderType type, string refId) : base()
        {
            this.Exchange = exchange;
            this.TimeCreated = DateTime.UtcNow;
            this.Symbol = instrument_id;
            this.Amount = amount;
            this.Price = price;
            this.Ordertype = type;
            this.RefId = refId;
            //this.Leverage = leverage;
        }

        public FutureOrder(FutureOrder other)
        {
            this.Copy(other);
        }

        public bool Equals(FutureOrder other)
        {
            if (other == null) return false;
            return base.Equals(other);
        }

        public void Copy(FutureOrder other)
        {
            if (other != null) 
                base.Copy(other);
        }

        //public bool EqualsExceptTimeLastAndPrevRefId(FutureOrder other)
        //{
        //    return (this.TimeCreated == other.TimeCreated &&
        //            //this.Leverage == other.Leverage &&
        //            this.Exchange == other.Exchange &&
        //            this.Symbol == other.Symbol &&
        //            this.Amount == other.Amount &&
        //            this.AvgPrice == other.AvgPrice &&
        //            this.DealAmount == other.DealAmount &&
        //            this.OrderId == other.OrderId &&
        //            this.Price == other.Price &&
        //            this.State == other.State &&
        //            this.RefId == other.RefId &&
        //            this.CommissionPaid == other.CommissionPaid &&
        //            this.Ordertype == other.Ordertype);
        //}

        //public void CopyExceptTimeLastAndPrevRefId(FutureOrder other)
        //{
        //    if (other == null) return;

        //    this.TimeCreated = other.TimeCreated;
        //    //this.Leverage = other.Leverage;
        //    this.Exchange = other.Exchange;
        //    this.Symbol = other.Symbol;
        //    this.Amount = other.Amount;
        //    this.AvgPrice = other.AvgPrice;
        //    this.DealAmount = other.DealAmount;
        //    this.OrderId = other.OrderId;
        //    this.Price = other.Price;
        //    this.State = other.State;
        //    this.RefId = other.RefId;
        //    this.CommissionPaid = other.CommissionPaid;
        //    this.Ordertype = other.Ordertype;
        //}
    }
}
