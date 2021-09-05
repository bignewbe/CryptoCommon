using CryptoCommon.Future.DataType;
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
    public class AccountBalance : EqualAndCopyUseReflection<AccountBalance>, IIdEqualCopy<AccountBalance>
    {
        public string Id { get { return Currency; } }
        public double Balance { get { return Available + Hold; } }
        public string Currency { get; set; }
        public double Available { get; set; }
        public double Hold { get; set; }

        public AccountBalance()
        {
        }

        public AccountBalance(AccountBalance other)
        {
            this.Copy(other);
        }
    }

    public interface IBalance
    {
        ConcurrentDictionary<string, AccountBalance> Balances { get; }
        AccountBalance GetBalance(string crypto);
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<AccountBalance> OnAccountBalanceUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<AccountBalance>> OnAccountBalanceListUpdated;
    }
}
