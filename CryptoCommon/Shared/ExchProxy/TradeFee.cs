using PortableCSharpLib.Interace;
using PortableCSharpLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public class TradeFee : EqualAndCopyUseReflection<TradeFee>, IIdEqualCopy<TradeFee>
    {
        public string Id => Symbol;
        public string Symbol { get; set; }
        public double Maker { get; set; }
        public double Taker { get; set; }
        public long Ts { get; set; }
    }
}
