using CryptoCommon.DataBase.Interface;
using CryptoCommon.Future.DataType;
using PortableCSharpLib.Interface;
using System;

namespace CryptoCommon.DataTypes
{
    public class FZOrder:IIdEqualCopy<FZOrder> // IIdAndName<FZOrder>, 
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

        //int IIdAndName<FZOrder>.Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime LocalTime { get { return TimeCreated.ToLocalTime(); } }
        public string Id { get { return OrderId; } }
        public string Exchange { get; set; }
        public string OrderId { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeLast { get; set; }
        public string Symbol { get; set; }
        public double Amount { get; set; }
        public double DealAmount { get; set; }
        public double AvgPrice { get; set; }
        public double Price { get; set; }
        public double TriggerPrice { get; set; }
        public double CommissionPaid { get; set; }
        public OrderState State { get; set; }
        public OrderType Ordertype { get; set; }
        //public TimeInForceType TimeInForce { get; set; }
        public string PrevRefId { get; set; }
        public string RefId { get; set; }

        public string AlgoId { get; set; }
        public double TPTriggerPrice { get; set; }
        public double TPPrice { get; set; }
        public double SLTriggerPrice { get; set; }
        public double SLPrice { get; set; }
        public double MinSz { get; set; } = 1;
        public double TickSz { get; set; }

        public string OrderMode { get; set; }
        public bool IsMarginOrder { get; set; }

        public string PxFormat { get; set; }
        public string AmtFormat { get; set; }

        public ExecutionType ExecutionType { get; set; } = ExecutionType.Standard;

        public FZOrder()
        {
        }

        public FZOrder(FZOrder other)
        {
            this.Copy(other);
        }

        public FZOrder(string exchange, string instrument_id, double amount, double price, OrderType type, string refId)
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

        public bool Equals(FZOrder other)
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
                this.TriggerPrice == other.TriggerPrice &&
                this.State == other.State &&
                this.Ordertype == other.Ordertype &&
                this.RefId == other.RefId &&
                this.CommissionPaid == other.CommissionPaid &&
                this.ExecutionType == other.ExecutionType &&
                this.AlgoId == other.AlgoId &&
                this.TPTriggerPrice == other.TPTriggerPrice &&
                this.TPPrice == other.TPPrice &&
                this.SLTriggerPrice == other.SLTriggerPrice &&
                this.PxFormat == other.PxFormat &&
                this.AmtFormat == other.AmtFormat &&
                //this.TimeInForce == other.TimeInForce &&
                this.SLPrice == other.SLPrice);
        }

        public void Copy(FZOrder other)
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
                this.TriggerPrice = other.TriggerPrice;
                this.State = other.State;
                this.Ordertype = other.Ordertype;
                this.RefId = other.RefId;
                this.CommissionPaid = other.CommissionPaid;
                this.ExecutionType = other.ExecutionType;
                this.AlgoId = other.AlgoId;
                this.TPTriggerPrice = other.TPTriggerPrice;
                this.TPPrice = other.TPPrice;
                this.SLTriggerPrice = other.SLTriggerPrice;
                this.SLPrice = other.SLPrice;
                this.PxFormat = other.PxFormat;
                this.AmtFormat = other.AmtFormat;
                //this.TimeInForce = other.TimeInForce;
            }
        }
    }
}
