using PortableCSharpLib.Interace;
using PortableCSharpLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortableCSharpLib;

namespace CryptoCommon.Shared.ExchProxy
{
    public enum BillType
    {
        NONE,
        Interest,
        FundingFee,
        Trade,
        Transfer,
        AutoTokenConversion,
        Liquidation,
        MarginTransfer,
        ADL,
        Clawback,
        SystemTokenConversion,
        StrategyTransfer,
        Delivery
    }

    public class Bill : EqualAndCopyUseReflection<Bill>, IIdEqualCopy<Bill>, ITs
    {
        public DateTime LocalTime => Ts.GetUTCFromMiliSeconds().ToLocalTime();

        public string Id { get; set; }
        public BillType Type { get; set; }
        public string Action { get; set; }
        public string Ccy { get; set; }
        public string Symbol { get; set; }
        public double Chg => Pnl + Fee + Interest + FundingFee;
        public double Pnl { get; set; }
        public double Fee { get; set; }
        public double Interest { get; set; }
        public double FundingFee { get; set; }
        public double Balance { get; set; }
        public double BalanceChg { get; set; }
        public long Ts { get; set; }

        public override string ToString()
        {
            return $"{Id}: {Type,-12} {Symbol,-20} {Ccy,-6} pnl = {Pnl,8:0.0##} fee = {Fee,8:0.0##} fun = {FundingFee,8:0.0##} chg = {BalanceChg,8:0.0##}";
        }
    }
}
