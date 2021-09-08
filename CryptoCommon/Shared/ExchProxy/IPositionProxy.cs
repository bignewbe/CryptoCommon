using PortableCSharpLib.Interace;
using PortableCSharpLib.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AccountPostion : EqualAndCopyUseReflection<AccountPostion>, IIdEqualCopy<AccountPostion>
    {
        public string Id => Symbol;

        public string InstType { get; set; }
        public string Symbol { get; set; }
        public string Ccy { get; set; }

        public double Pos { get; set; }
        public string PosSide { get; set; }
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

    public class FZPosition: EqualAndCopyUseReflection<FZPosition>, IIdEqualCopy<FZPosition>
    {
        public string Id => Symbol;
        public string Symbol
        {
            get
            {
                if (Long != null)
                    return Long.Symbol;
                else if (Short != null)
                    return Short.Symbol;
                return null;
            }
        }
        public AccountPostion Long { get; set; }
        public AccountPostion Short { get; set; }
    }

    public interface IPositionProxy
    {
        ConcurrentDictionary<string, FZPosition> Positions { get; }
        FZPosition GetPosition(string symbol);
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZPosition> OnAccountPositionUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZPosition>> OnAccountPositionListUpdated;
    }
}
