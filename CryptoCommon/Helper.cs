using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon
{
    public class Helper
    {
        public static double ConvertStrToDouble(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return double.Parse(str);
        }

        public static int ConvertStrToInt(string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            return int.Parse(str);
        }

        public static InstrumentType GetInstTypeFromSymbol(string symbol)
        {
            if (symbol.Contains("SWAP"))
                return InstrumentType.SWAP;

            if (symbol.Split('_').Length == 3 || symbol.Split('-').Length == 3)
                return InstrumentType.FUTURES;

            return InstrumentType.SPOT;
        }

        public static string GetInstTypeStr(string symbol)
        {
            var instType = Helper.GetInstTypeFromSymbol(symbol);
            var it = instType switch
            {
                InstrumentType.SPOT => "Spot",
                InstrumentType.FUTURES => "Future",
                InstrumentType.SWAP => "Swap",
                _ => null
            };
            return it;
        }
    }
}
