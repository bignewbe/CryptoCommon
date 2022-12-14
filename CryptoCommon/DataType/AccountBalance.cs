using PortableCSharpLib.Interface;
using PortableCSharpLib.Util;
using System;
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
        public double Borrowed { get; set; }
        public double Interest { get; set; }
        public string Currency { get; set; }
        public double Available { get; set; }
        public double Hold { get; set; }
        public double Price { get; set; }
        public double UsdtVal => Balance * Price;

        public AccountBalance()
        {
        }

        public AccountBalance(AccountBalance other)
        {
            this.Copy(other);
        }
    }
}
