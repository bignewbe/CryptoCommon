using CryptoCommon.DataTypes;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface ISpotAccountProxy
    {
        ConcurrentDictionary<string, SpotBalance> Balances { get; }
        SpotBalance GetBalance(string crypto);
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotBalance> OnCurrencyBalanceUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotBalance>> OnAccountBalanceUpdated;
    }
}
