using PortableCSharpLib.Interface;
using PortableCSharpLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public class AccountPosition : EqualAndCopyUseReflection<AccountPosition>, IIdEqualCopy<AccountPosition>
    {
        public string Id => $"{Symbol}x{PosSide}";

        public string InstType { get; set; }
        public string Symbol { get; set; }
        public string Ccy { get; set; }

        //public double Pos { get; set; }
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
        public double UPnl { get; set; }   //unrealized PNL
        public int Leverage { get; set; }

        public AccountPosition()
        {
        }

        public AccountPosition(AccountPosition other)
        {
            this.Copy(other);
        }
    }
}
