using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    /// <summary>
    /// currently used in TraderService to transfer SwingParam to web, because SwingParam is too large
    /// </summary>
    public class TraderParam
    {
        [JsonIgnore]
        public string Id => ParamId;
        [JsonIgnore]
        public string ParamId => $"{GivenId}".Replace("-", "").Replace("_", "");
        [JsonIgnore]
        public string QuoteId => $"{Symbol}_{Interval}";

        public string Exchange { get; set; }
        public string Symbol { get; set; }
        public string GivenId { get; set; }
        public int Interval { get; set; }
        public double QtyEach { get; set; }
        public CryptoCommon.DataTypes.EnumType TradeModel { get; set; }

        public TraderParam()
        {
        }

        public TraderParam(TraderParam other)
        {
            this.Copy(other);
        }

        public void Copy(TraderParam other)
        {
            if (other == null) return;
            Exchange = other.Exchange;
            Symbol = other.Symbol;
            GivenId = other.GivenId;
            Interval = other.Interval;
            QtyEach = other.QtyEach;
            TradeModel = other.TradeModel;
        }
    }
}
