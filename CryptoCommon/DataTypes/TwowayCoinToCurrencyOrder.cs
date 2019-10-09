using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    //buy crypto1@exchange1, sell crypto1@exchagne2
    //buy crypto2@exchange2, sell crypto2@exchange1
    public class TwowayCoinToCurrencyOrder : IEquatable<TwowayCoinToCurrencyOrder>
    {
        public DateTime LocalTime { get { return CreateTime.ToLocalTime(); } }
        public bool HasOrderIds
        {
            get
            {
                return
                    (!string.IsNullOrEmpty(this.OrderId_Crypto1Exchange1) ||
                     !string.IsNullOrEmpty(this.OrderId_Crypto1Exchange2) ||
                     !string.IsNullOrEmpty(this.OrderId_Crypto2Exchange2) ||
                     !string.IsNullOrEmpty(this.OrderId_Crypto2Exchange1));
            }
        }

        public bool HasCompleteOrderIds
        {
            get
            {
                return
                    (!string.IsNullOrEmpty(this.OrderId_Crypto1Exchange1) &&
                     !string.IsNullOrEmpty(this.OrderId_Crypto1Exchange2) &&
                     !string.IsNullOrEmpty(this.OrderId_Crypto2Exchange2) &&
                     !string.IsNullOrEmpty(this.OrderId_Crypto2Exchange1));
            }
        }

        public string RefId { get; set; }
        public DateTime CreateTime { get; set; }
        public string Exchange1 { get; set; }
        public string Exchange2 { get; set; }
        public string Crypto1 { get; set; }
        public string Crypto2 { get; set; }
        public double Amount1 { get; set; }
        public double Amount2 { get; set; }
        
        public string OrderId_Crypto1Exchange1 { get; set; }
        public string OrderId_Crypto1Exchange2 { get; set; }
        public string OrderId_Crypto2Exchange2 { get; set; }
        public string OrderId_Crypto2Exchange1 { get; set; }


        //public int Status_BuyCrypto1Exchange1 { get; set; }
        //public int Status_SellCrypto1Exchange2 { get; set; }
        //public int Status_BuyCrypto2Exchange2 { get; set; }
        //public int Status_SellCrypto2Exchange1 { get; set; }


        public double Crypto1Delta { get { return AmountCrypto1Exchange1 - AmountCrypto1Exchange2; } }
        public double Crypto2Delta { get { return AmountCrypto2Exchange2 - AmountCrypto2Exchange1; } }
        public double ProfitExchange1 { get { return AmountCrypto2Exchange1 * PriceCrypto2Exchange1 - AmountCrypto1Exchange1 * PriceCrypto1Exchange1; } }
        public double ProfitExchange2 { get { return AmountCrypto1Exchange2 * PriceCrypto1Exchange2 - AmountCrypto2Exchange2 * PriceCrypto2Exchange2; } }

        public double ProfitCryptoCurrency1 { get { return Crypto1Delta * PriceCrypto1Exchange1 + Crypto2Delta * PriceCrypto2Exchange1; } }
        public double ProfitCryptoCurrency2 { get { return Crypto1Delta * PriceCrypto1Exchange2 + Crypto2Delta * PriceCrypto2Exchange2; } }

        public double AmountCrypto1Exchange1 { get; set; }
        public double AmountCrypto1Exchange2 { get; set; }
        public double AmountCrypto2Exchange2 { get; set; }
        public double AmountCrypto2Exchange1 { get; set; }

        public double PriceCrypto1Exchange1 { get; set; }
        public double PriceCrypto1Exchange2 { get; set; }
        public double PriceCrypto2Exchange2 { get; set; }
        public double PriceCrypto2Exchange1 { get; set; }

        public bool Equals(TwowayCoinToCurrencyOrder other)
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

        public void Copy(TwowayCoinToCurrencyOrder other)
        {
            if (other == null) return;
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p=>p.CanWrite).ToList();
            foreach (var p in properties)
            {
                var v2 = p.GetValue(other);
                p.SetValue(this, v2);
            }
        }

        public bool IsContainOrderId(string orderId)
        {
            return (
                this.OrderId_Crypto1Exchange1 == orderId ||
                this.OrderId_Crypto1Exchange2 == orderId ||
                this.OrderId_Crypto2Exchange2 == orderId ||
                this.OrderId_Crypto2Exchange1 == orderId);
        }

        public bool UpdateOrder(SpotOrder order)
        {
            var updated = false;
            if (order.OrderId == this.OrderId_Crypto1Exchange1)
            {
                if (this.AmountCrypto1Exchange1 != order.DealAmount) { updated = true; this.AmountCrypto1Exchange1 = order.DealAmount; }
                if (this.PriceCrypto1Exchange1 != order.AvgPrice) { updated = true; this.PriceCrypto1Exchange1 = order.AvgPrice; }
            }
            else if (order.OrderId == this.OrderId_Crypto1Exchange2)
            {
                if (this.AmountCrypto1Exchange2 != order.DealAmount) { updated = true; this.AmountCrypto1Exchange2 = order.DealAmount; }
                if (this.PriceCrypto1Exchange2 != order.AvgPrice) { updated = true; this.PriceCrypto1Exchange2 = order.AvgPrice; }
            }
            else if (order.OrderId == this.OrderId_Crypto2Exchange1)
            {
                if (this.AmountCrypto2Exchange1 != order.DealAmount) { updated = true; this.AmountCrypto2Exchange1 = order.DealAmount; }
                if (this.PriceCrypto2Exchange1 != order.AvgPrice) { updated = true; this.PriceCrypto2Exchange1 = order.AvgPrice; }
            }
            else if (order.OrderId == this.OrderId_Crypto2Exchange2)
            {
                if (this.AmountCrypto2Exchange2 != order.DealAmount) { updated = true; this.AmountCrypto2Exchange2 = order.DealAmount; }
                if (this.PriceCrypto2Exchange2 != order.AvgPrice) { updated = true; this.PriceCrypto2Exchange2 = order.AvgPrice; }
            }
            return updated;
        }
    }
}
