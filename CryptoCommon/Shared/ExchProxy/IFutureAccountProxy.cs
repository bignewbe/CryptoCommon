using CryptoCommon.Future;
using CryptoCommon.Future.DataType;
using CryptoCommon.Future.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IFutureAccountProxy
    {
        ConcurrentDictionary<string, FuturePosition> Positions { get; }
        ConcurrentDictionary<string, FutureBalance> Balances { get; }
        FutureBalance GetBalance(string crypto);
        FuturePosition GetPosition(string symbol);
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FutureBalance> OnSingleBalanceUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FuturePosition> OnSinglePositionUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FutureBalance>> OnBatchBalanceUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FuturePosition>> OnBatchPositionUpdated;
    }
}
