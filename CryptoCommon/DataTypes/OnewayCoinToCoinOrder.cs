using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public class OnewayCoinToCoinOrder
    {
        public DateTime LocalTime { get { return CreateTime.ToLocalTime(); } }
        public bool HasOrderIds
        {
            get
            {
                return
                    (!string.IsNullOrEmpty(this.OrderIdExchange1) ||
                     !string.IsNullOrEmpty(this.OrderIdExchange2));
            }
        }

        public bool HasCompleteOrderIds
        {
            get
            {
                return
                    (!string.IsNullOrEmpty(this.OrderIdExchange1) &&
                     !string.IsNullOrEmpty(this.OrderIdExchange2));
            }
        }

        public string RefId { get; set; }
        public DateTime CreateTime { get; set; }
        public string Exchange1 { get; set; }
        public string Exchange2 { get; set; }
        public string Symbol { get; set; }
        public double Amount { get; set; }

        public string OrderIdExchange1 { get; set; }
        public string OrderIdExchange2 { get; set; }
        public double AmountExchange1 { get; set; }
        public double AmountExchange2 { get; set; }
        public double PriceExchange1 { get; set; }
        public double PriceExchange2 { get; set; }
        
        //change of the crypto
        public double CryptoDelta { get { return AmountExchange1 - AmountExchange2; } }
        //change of base currency
        public double BaseCurrencyDelta{ get { return AmountExchange2 * PriceExchange2 - AmountExchange1 * PriceExchange1; } }
        //change of crypto in terms of base currency
        public double CryptoValueDelta { get { return CryptoDelta * PriceExchange1; } }

        public bool Equals(OnewayCoinToCoinOrder other)
        {
            if (other == null) return false;

            var properties = this.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            foreach (var p in properties)
            {
                var v1 = p.GetValue(this);
                var v2 = p.GetValue(other);
                if (v1 == null && v2 == null) continue;
                if (v1 == null || v2 == null) return false;
                if (!v1.Equals(v2)) return false;
            }
            return true;
        }

        public void Copy(OnewayCoinToCoinOrder other)
        {
            if (other == null) return;
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.CanWrite).ToList();
            foreach (var p in properties)
            {
                var v2 = p.GetValue(other);
                p.SetValue(this, v2);
            }
        }

        public bool IsContainOrderId(string orderId)
        {
            return (
                this.OrderIdExchange1 == orderId ||
                this.OrderIdExchange2 == orderId);
        }

        public bool UpdateOrder(Order order)
        {
            var updated = false;
            if (order.OrderId == this.OrderIdExchange1)
            {
                if (this.AmountExchange1 != order.DealAmount) { updated = true; this.AmountExchange1 = order.DealAmount; }
                if (this.PriceExchange1 != order.AvgPrice) { updated = true; this.PriceExchange1 = order.AvgPrice; }
            }
            else if (order.OrderId == this.OrderIdExchange2)
            {
                if (this.AmountExchange2 != order.DealAmount) { updated = true; this.AmountExchange2 = order.DealAmount; }
                if (this.PriceExchange2 != order.AvgPrice) { updated = true; this.PriceExchange2 = order.AvgPrice; }
            }
            return updated;
        }
    }
}
