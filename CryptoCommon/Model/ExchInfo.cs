using CryptoCommon.DataTypes;
using CryptoCommon.Interface;
using CryptoCommon.Shared.ExchProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Model
{
    public class ExchInfo : IExchInfo
    {
        public string Exchange { get; private set; }
        Dictionary<string, Instrument> _instruments;

        public ExchInfo(string exchange, List<Instrument> instruments)
        {
            this.Exchange = exchange;
            _instruments = instruments.ToDictionary(x => x.Symbol, x => x);
        }
        public ExchInfo(string exchange, Dictionary<string, Instrument> instruments)
        {
            this.Exchange = exchange;
            _instruments = instruments;
        }

        //special for Okex
        public double GetLotSz(string symbol) => _instruments[symbol].LotSz;
        public double GetTickSz(string symbol) => _instruments[symbol].TickSz;
        public double GetQtySz(string symbol) => _instruments[symbol].MinSz;
        public string GetPriceFmt(string symbol)
        {
            var d = _instruments[symbol].TickSz;
            return GetStrFormat(d);
        }
        public string GetQtyFmt(string symbol)
        {
            if (Exchange == "Okex")
            {
                var it = Helper.GetInstTypeFromSymbol(symbol);
                if (it == InstrumentType.SPOT)
                {
                    var d = _instruments[symbol].MinSz;
                    return GetStrFormat(d);
                }
                else
                {
                    return GetStrFormat(_instruments[symbol].LotSz);
                }
            }
            else
            {
                var d = _instruments[symbol].MinSz;
                return GetStrFormat(d);
            }
        }
        private static string GetStrFormat(double d)
        {
            var fmt = "0";
            if (d >= 0.999)
                fmt = "0";
            else
            {
                fmt += ".0";
                long f = 10;
                while (d * f < 0.999)
                {
                    fmt += "0";
                    f *= 10;
                }
            }
            return fmt;
        }
    }
}
