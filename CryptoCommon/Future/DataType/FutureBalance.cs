using CryptoCommon.DataTypes;
using PortableCSharpLib.Interace;

namespace CryptoCommon.Future.DataType
{
    public class FutureBalance : IIdEqualCopy<FutureBalance> //EqualAndCopyUseReflection<FutureBalance>
    {
        public FutureBalance()
        {
        }
        public FutureBalance(FutureBalance other)
        {
            this.Copy(other);
        }

        //"equity": "1.64393282",
        //    "margin_mode": "fixed",
        //    "total_avail_balance": "1.64393282"
        //public string Id { get { return this.Currency; } }
        //public string Currency { get; set; }
        //public double Available { get; set; }

        public string Id { get { return Currency; } }
        //public double Balance { get { return Available + Hold; } }
        public string Currency { get; set; }
        public double? equity { get; set; }               //权益 = total_available_balance
        public double? total_available_balance { get; set; }
        public string margin_mode { get; set; }
        public double? available_margin { get; set; }
        public double? can_withdraw { get; set; }
        /// <summary>
        /// = occupied margin = margin_for_unfilled + margin_frozen for open order
        /// </summary>
        //public double margin { get { return margin_for_unfilled + margin_frozen; } } //保证金 
        public double? margin { get; set; }               // = 持仓保证金 + 冻结保证金          
        public double? margin_for_unfilled { get; set; }  //持仓保证金 = qty * facevalue / current price / leverage
        public double? margin_frozen { get; set; }        //冻结保证金 = qty * facevalue / order price / leverage
        public double? margin_ratio { get; set; }         //保证金率
        public double? maint_margin_ratio { get; set; }   //最小维持保证金率
        public double? realized_pnl { get; set; }
        public double? unrealized_pnl { get; set; }

        //public string liqui_fee_rate { get; set; }
        //public string liqui_mode { get; set; }
        //for spot
        //public string Currency { get; set; }
        //public string Id { get; set; }
        //public double Available { get; set; }
        //public double Balance { get { return Available + Hold; } }
        //public double Hold { get; set; }

        public void Copy(FutureBalance other)
        {
            if (other == null) return;

            this.Currency = other.Currency;
            this.equity = other.equity;
            this.available_margin = other.available_margin;
            this.margin = other.margin;
            this.margin_mode = other.margin_mode;
            this.margin_for_unfilled = other.margin_for_unfilled;
            this.margin_frozen = other.margin_frozen;
            this.realized_pnl = other.realized_pnl;
            this.unrealized_pnl = other.unrealized_pnl;
        }

        public bool Equals(FutureBalance other)
        {
            if (other == null) return false;
            return (
                    this.Currency == other.Currency &&
                    this.equity == other.equity &&
                    this.available_margin == other.available_margin &&
                    this.margin == other.margin &&
                    this.margin_mode == other.margin_mode &&
                    this.margin_for_unfilled == other.margin_for_unfilled &&
                    this.margin_frozen == other.margin_frozen &&
                    this.realized_pnl == other.realized_pnl &&
                    this.unrealized_pnl == other.unrealized_pnl);
        }
    }
}
