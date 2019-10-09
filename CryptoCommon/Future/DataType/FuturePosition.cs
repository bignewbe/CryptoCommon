using PortableCSharpLib.Interace;
using PortableCSharpLib.Util;
using System;

namespace CryptoCommon.Future.Interface
{
    public class FuturePosition : EqualAndCopyUseReflection<FuturePosition>, IIdEqualCopy<FuturePosition>
    {
        public FuturePosition()
        {
        }
        public FuturePosition(FuturePosition other)
        {
            this.Copy(other);
        }
        //public double Margin_mode { get {
        public double Realised_pnl { get { return FutureOrder.ConvertStrToDouble(realised_pnl); } }
        public double Leverage { get { return FutureOrder.ConvertStrToDouble(leverage); } }

        public int Long_qty { get { return FutureOrder.ConvertStrToInt(long_qty); } }
        public int Long_avail_qty { get { return FutureOrder.ConvertStrToInt(long_avail_qty); } }
        public double Long_avg_cost { get { return FutureOrder.ConvertStrToDouble(long_avg_cost); } }
        public double Long_settlement_price { get { return FutureOrder.ConvertStrToDouble(long_settlement_price); } }
        public double Long_margin { get { return FutureOrder.ConvertStrToDouble(long_margin); } }
        public double Long_liqui_price { get { return FutureOrder.ConvertStrToDouble(long_liqui_price); } }
        public double Long_pnl_ratio { get { return FutureOrder.ConvertStrToDouble(long_pnl_ratio); } }
        public double Long_leverage { get { return Math.Max(this.Leverage, FutureOrder.ConvertStrToDouble(long_leverage)); } }
        public double Long_margin_ratio { get { return FutureOrder.ConvertStrToDouble(long_margin_ratio); } }
        public double Long_maint_margin_ratio { get { return FutureOrder.ConvertStrToDouble(long_maint_margin_ratio); } }
        public double Long_pnl { get { return FutureOrder.ConvertStrToDouble(long_pnl); } }
        public double Long_unrealised_pnl { get { return FutureOrder.ConvertStrToDouble(long_unrealised_pnl); } }

        public int Short_qty { get { return FutureOrder.ConvertStrToInt(short_qty); } }
        public int Short_avail_qty { get { return FutureOrder.ConvertStrToInt(short_avail_qty); } }
        public double Short_avg_cost { get { return FutureOrder.ConvertStrToDouble(short_avg_cost); } }
        public double Short_settlement_price { get { return FutureOrder.ConvertStrToDouble(short_settlement_price); } }
        public double Short_margin { get { return FutureOrder.ConvertStrToDouble(short_margin); } }
        public double Short_liqui_price { get { return FutureOrder.ConvertStrToDouble(short_liqui_price); } }
        public double Short_pnl_ratio { get { return FutureOrder.ConvertStrToDouble(short_pnl_ratio); } }
        public double Short_leverage { get { return Math.Max(this.Leverage, FutureOrder.ConvertStrToDouble(short_leverage)); } }
        public double Short_margin_ratio { get { return FutureOrder.ConvertStrToDouble(short_margin_ratio); } }
        public double Short_maint_margin_ratio { get { return FutureOrder.ConvertStrToDouble(short_maint_margin_ratio); } }
        public double Short_pnl { get { return FutureOrder.ConvertStrToDouble(short_pnl); } }
        public double Short_unrealised_pnl { get { return FutureOrder.ConvertStrToDouble(short_unrealised_pnl); } }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public string margin_mode { get; set; }
        public string realised_pnl { get; set; }
        public string leverage { get; set; }
        public string instrument_id { get; set; }

        public string long_qty { get; set; }
        public string long_avail_qty { get; set; }
        public string long_avg_cost { get; set; }
        public string long_settlement_price { get; set; }
        public string long_margin { get; set; }
        public string long_liqui_price { get; set; }
        public string long_pnl_ratio { get; set; }
        public string long_leverage { get; set; }
        public string long_margin_ratio { get; set; }
        public string long_maint_margin_ratio { get; set; }
        public string long_pnl { get; set; }
        public string long_unrealised_pnl { get; set; }

        public string short_qty { get; set; }
        public string short_avail_qty { get; set; }
        public string short_avg_cost { get; set; }
        public string short_settlement_price { get; set; }
        public string short_margin { get; set; }
        public string short_liqui_price { get; set; }
        public string short_pnl_ratio { get; set; }
        public string short_leverage { get; set; }
        public string short_margin_ratio { get; set; }
        public string short_maint_margin_ratio { get; set; }
        public string short_pnl { get; set; }
        public string short_unrealised_pnl { get; set; }

        public string Id { get { return instrument_id; } }
        public void Copy(FuturePosition other)
        {
            base.Copy(other);
        }

        public bool Equals(FuturePosition other)
        {
            return base.Equals(other);
        }
    }

    //public class FuturePosition
    //{
    //    public string instrument_id { get; set; }
    //    public DateTime created_at { get; set; }
    //    public DateTime updated_at { get; set; }
    //    public string margin_mode { get; set; }
    //    public double realised_pnl { get; set; }
    //    public int leverage { get; set; }

    //    public int long_qty { get; set; }
    //    public int long_avail_qty { get; set; }
    //    public double long_margin { get; set; }
    //    public double long_liqui_price { get; set; }
    //    public double long_pnl_ratio { get; set; }
    //    public double long_avg_cost { get; set; }
    //    public double long_settlement_price { get; set; }
    //    public double long_margin_ratio { get; set; }
    //    public double long_maint_margin_ratio { get; set; }
    //    public double long_pnl { get; set; }
    //    public double long_unrealised_pnl { get; set; }
    //    public int long_leverage { get; set; }

    //    public int short_qty { get; set; }
    //    public int short_avail_qty { get; set; }
    //    public double short_margin { get; set; }
    //    public double short_liqui_price { get; set; }
    //    public double short_pnl_ratio { get; set; }
    //    public double short_avg_cost { get; set; }
    //    public double short_settlement_price { get; set; }
    //    public double short_margin_ratio { get; set; }
    //    public double short_maint_margin_ratio { get; set; }
    //    public double short_pnl { get; set; }
    //    public double short_unrealised_pnl { get; set; }
    //    public int short_leverage { get; set; }
    //}
}
