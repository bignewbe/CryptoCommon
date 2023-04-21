using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interface
{
    public interface IExchInfo
    {
        string Exchange { get; }
        //double GetLotSz(string symbol);
        double GetTickSz(string symbol);
        double GetMinSz(string symbol);
        string GetPriceFmt(string symbol);
        string GetQtyFmt(string symbol);
    }
}
