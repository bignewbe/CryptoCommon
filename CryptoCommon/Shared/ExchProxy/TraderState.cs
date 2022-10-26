using CryptoCommon.DataTypes;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public string Id { get { return ParamId; } }

        [JsonProperty(PropertyName = "S")]
        public string Symbol { get; set; }
        [JsonProperty(PropertyName = "P")]
        public string ParamId { get; set; }
        [JsonProperty(PropertyName = "I")]
        public int Interval { get; set; }

        #region spot
        [JsonIgnore]
        public double? _prevSoldPrice { get; set; }
        [JsonIgnore]
        public double? _prevBoughtPrice { get; set; }
        [JsonIgnore]
        public long _lastRefreshLocalStatusTime { get; set; }
        [JsonIgnore]
        public long? _prevCreatedTimeBought { get; set; }
        [JsonIgnore]
        public long? _prevCreatedTimeSold { get; set; }
        [JsonIgnore]
        public bool _isInitialized { get; } 
        [JsonIgnore]
        public FZOrder _lastOrder { get; set; }

        [JsonIgnore]
        public double ExptC1 { get; set; }
        [JsonProperty(PropertyName = "a")]
        public double AvailC1 { get; set; }
        [JsonProperty(PropertyName = "b")]
        public double AvailC2 { get; set; }
        [JsonProperty(PropertyName = "c")]
        public double Ratio { get; set; }
        [JsonProperty(PropertyName = "d")]
        public int NumOpenBuyOrder { get; set; }
        [JsonProperty(PropertyName = "e")]
        public int NumOpenSellOrder { get; set; }
        [JsonProperty(PropertyName = "f")]
        public double QtySold { get; set; }
        [JsonProperty(PropertyName = "g")]
        public int NumSoldOrder { get; set; }
        [JsonProperty(PropertyName = "h")]
        public double HoldC2 { get; set; }
        [JsonProperty(PropertyName = "i")]
        public double QtyBought { get; set; }
        [JsonProperty(PropertyName = "j")]
        public double AvgSoldPrice { get; set; }
        [JsonProperty(PropertyName = "k")]
        public double AvgBoughtPrice { get; set; }
        [JsonProperty(PropertyName = "l")]
        public double LastPrice { get; set; }
        [JsonProperty(PropertyName = "m")]
        public int NumBoughtOrder { get; set; }

        [JsonIgnore]
        public double ExptC2 { get; set; }
        [JsonProperty(PropertyName = "n")]
        public double NetC1 { get; set; }
        [JsonProperty(PropertyName = "o")]
        public double NetC2 { get; set; }
        [JsonProperty(PropertyName = "p")]
        public bool IsTraderStarted { get; set; }
        [JsonProperty(PropertyName = "q")]
        public double QtySellOpen { get; }
        [JsonProperty(PropertyName = "r")]
        public double QtyBuyOpen { get; }
        public string C2 { get; }
        public string C1 { get; }
        [JsonProperty(PropertyName = "s")]
        public double HoldC1 { get; set; }
        public double Pnl { get; }
        #endregion

        #region future/swap
        [JsonProperty(PropertyName = "t")]
        public double QtyLongOpened { get; set; }
        [JsonProperty(PropertyName = "u")]
        public double QtyLongClosed { get; set; }
        [JsonProperty(PropertyName = "v")]
        public double QtyShortOpened { get; set; }
        [JsonProperty(PropertyName = "w")]
        public double QtyShortClosed { get; set; }
        [JsonProperty(PropertyName = "x")]
        public double QtyOpenLongStanding { get; set; }
        [JsonProperty(PropertyName = "y")]
        public double QtyCloseLongStanding { get; set; }
        [JsonProperty(PropertyName = "z")]
        public double QtyOpenShortStanding { get; set; }
        [JsonProperty(PropertyName = "A")]
        public double QtyCloseShortStanding { get; set; }
        [JsonProperty(PropertyName = "B")]
        public double QtyLong { get; set; }
        [JsonProperty(PropertyName = "C")]
        public double QtyShort { get; set; }
        [JsonProperty(PropertyName = "D")]
        public double Balance { get; set; }
        [JsonProperty(PropertyName = "E")]
        public double AvgShortPrice { get; set; }
        [JsonProperty(PropertyName = "F")]
        public double AvgLongPrice { get; set; }
        #endregion
    }
}
