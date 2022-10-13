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
    public class TradeState : EqualAndCopyUseReflection<TradeState>, IIdEqualCopy<TradeState>
    {
        public TradeState()
        {
        }
        public TradeState(string paramId, string symbol)
        {
            ParamId = paramId;
            Symbol = symbol;
            var splitter = Symbol.Contains("-") ? "-" : "_";
            C1 = Symbol.Split(splitter)[0];
            C2 = Symbol.Split(splitter)[1];
        }

        public string Symbol { get; set; }
        public string ParamId { get; set; }
        public string Id { get { return ParamId; } }
        public int Interval { get; set; }

        #region spot
        public double? _prevSoldPrice { get; set; }
        public double? _prevBoughtPrice { get; set; }
        public long _lastRefreshLocalStatusTime { get; set; }
        public long? _prevCreatedTimeBought { get; set; }
        public long? _prevCreatedTimeSold { get; set; }
        public bool _isInitialized { get; } 
        public FZOrder _lastOrder { get; set; }

        public double ExptC1 { get; set; }
        public double AvailC1 { get; set; }
        public double AvailC2 { get; set; }
        public double Ratio { get; set; }
        public int NumOpenBuyOrder { get; set; }
        public int NumOpenSellOrder { get; set; }
        public double QtySold { get; set; }
        public int NumSoldOrder { get; set; }
        public double HoldC2 { get; set; }
        public double QtyBought { get; set; }
        public double AvgSoldPrice { get; set; }
        public double AvgBoughtPrice { get; set; }
        public double LastPrice { get; set; }
        public int NumBoughtOrder { get; set; }
        public double ExptC2 { get; set; }
        public double NetC1 { get; set; }
        public double NetC2 { get; set; }
        public bool IsTraderStarted { get; set; }
        public double QtySellOpen { get; }
        public double QtyBuyOpen { get; }
        public string C2 { get; }
        public string C1 { get; }
        public double HoldC1 { get; set; }
        public double Pnl { get; }
        #endregion

        #region future/swap
        public double QtyLongOpened { get; set; }
        public double QtyLongClosed { get; set; }
        public double QtyShortOpened { get; set; }
        public double QtyShortClosed { get; set; }
        public double QtyOpenLongStanding { get; set; }
        public double QtyCloseLongStanding { get; set; }
        public double QtyOpenShortStanding { get; set; }
        public double QtyCloseShortStanding { get; set; }
        public double QtyLong { get; set; }
        public double QtyShort { get; set; }
        public double Balance { get; set; }
        public double AvgShortPrice { get; set; }
        public double AvgLongPrice { get; set; }
        #endregion
    }
}
