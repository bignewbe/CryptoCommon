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
        public string Ccy { get; set; }
        public string Side { get; set; }
        public double Pos { get; set; }
        public double AvailPos { get; set; }
        public double AvgPrice { get; set; }
        public double LiqPrice { get; set; }
        public double Interest { get; set; }
        public long CreatedTime { get; set; }
        public long UpdatedTime { get; set; }
        public string MarginMode { get; set; }
        public double Margin { get; set; }
        public double Mmr { get; set; }
    }

    public interface IPositionProxy
    {
        ConcurrentDictionary<string, AccountPostion> Positions { get; }
        AccountPostion GetPosition(string symbol);
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<AccountPostion> OnAccountPositionUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<AccountPostion>> OnAccountPositionListUpdated;
    }
}
