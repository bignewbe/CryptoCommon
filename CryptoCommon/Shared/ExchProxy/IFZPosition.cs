using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AccountPostion
    {
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public string Side { get; set; }
        public string pos { get; set; }
        public string availPos { get; set; }
        public string avgPx { get; set; }
        public string liqPx { get; set; }
        public string interest { get; set; }
        public string cTime { get; set; }
        public string uTime { get; set; }
        public string pTime { get; set; }
        public string mgnMode { get; set; }
        public string margin { get; set; }
        public string mmr { get; set; }
    }

    public class AccountPostionSR
    {
        public string instType { get; set; }
        public string instId { get; set; }
        public string posCcy { get; set; }
        public string pos { get; set; }
        public string posSide { get; set; }
        public string posId { get; set; }
        public string availPos { get; set; }
        public string avgPx { get; set; }
        public string liqPx { get; set; }
        public string interest { get; set; }
        public string cTime { get; set; }
        public string uTime { get; set; }
        public string pTime { get; set; }
        public string mgnMode { get; set; }
        public string margin { get; set; }
        public string mmr { get; set; }

        public string adl { get; set; }
        public string ccy { get; set; }
        public string deltaBS { get; set; }
        public string deltaPA { get; set; }
        public string gammaBS { get; set; }
        public string gammaPA { get; set; }
        public string imr { get; set; }
        public string last { get; set; }
        public string lever { get; set; }
        public string liab { get; set; }
        public string liabCcy { get; set; }
        public string mgnRatio { get; set; }
        public string notionalUsd { get; set; }
        public string optVal { get; set; }
        public string thetaBS { get; set; }
        public string thetaPA { get; set; }
        public string tradeId { get; set; }
        public string upl { get; set; }
        public string uplRatio { get; set; }
        public string vegaBS { get; set; }
        public string vegaPA { get; set; }
    }

    public interface IPosition
    {
        ConcurrentDictionary<string, AccountPostion> Positions { get; }
        AccountPostion GetPosition(string symbol);
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<AccountPostion> OnAccountPositionUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<AccountPostion>> OnAccountPositionListUpdated;
    }
}
