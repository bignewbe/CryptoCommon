using CryptoCommon.DataTypes;
using PortableCSharpLib.Interface;
using PortableCSharpLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public enum SymbolState
    {
        NONE,
        live,
        suspend,
        expired,
        preopen
    }

    public enum SymbolPeriod
    {
        NONE,
        this_week,
        next_week,
        quarter,
        next_quarter
    }

    public class MarginInterest : EqualAndCopyUseReflection<MarginInterest>, IIdEqualCopy<MarginInterest>
    { 
        public double Quota { get; set; }
        public double Rate { get; set; }
        public string Ccy { get; set; }
        public string Id => Ccy;
    }

    public class Instrument
    {
        public InstrumentType InstType { get; set; }
        public string Symbol { get; set; }
        public double TickSz { get; set; }
        public double LotSz { get; set; }
        public double MinSz { get; set; }
        //public double FaceValue { get; set; }
        //public double MultiplyFactor { get; set; }
        public SymbolPeriod Period { get; set; }
        public SymbolState State { get; set; }
        public int MaxLeverage { get; set; }
    }

    public class MaxLoan : EqualAndCopyUseReflection<MaxLoan>, IIdEqualCopy<MaxLoan>
    {
        public string Symbol { get; set; }
        public string MgnMode { get; set; }
        public string MgnCcy { get; set; }
        public double Loan { get; set; }
        public string Ccy { get; set; }
        public string Side { get; set; }

        public string Id => Symbol;
    }

    public class MaxSize : EqualAndCopyUseReflection<MaxSize>, IIdEqualCopy<MaxSize>
    {
        /// <summary>
        /// spot: amount of c1 that we can buy, for ETH-USDT c1 = ETH, c2 = USDT
        /// swap: amount of coin / minSz, e.g., 300 for ETH-USDT-SWAP means that we can long/short 300 * 0.1 = 30 ETH
        /// </summary>
        public double AvailBuy { get; set; }
        /// <summary>
        /// spot: amount of c1 that we can sell, for ETH-USDT c1 = ETH, c2 = USDT
        /// swap: amount of coin / minSz, e.g., 300 for ETH-USDT-SWAP means that we can long/short 300 * 0.1 = 30 ETH
        /// </summary>
        public double AvailSell { get; set; }
        public string Symbol { get; set; }
        public string Ccy { get; set; }

        public string Id => Symbol;
    }

}
