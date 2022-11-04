using CryptoCommon.DataTypes;
using PortableCSharpLib.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public class FundingRate : IIdEqualCopy<FundingRate>
    {
        public string Id => Symbol;
        public double TotalRate => Rate + NextRate;

        public InstrumentType InstType { get; set; }
        public string Symbol { get; set; }
        public double Rate { get; set; }
        public double NextRate { get; set; }
        public long FundingTime { get; set; }
        public long NextFundingTime { get; set; }

        public void Copy(FundingRate other)
        {
            if (other != null)
            {
                NextFundingTime = other.NextFundingTime;
                FundingTime = other.FundingTime;
                InstType = other.InstType;
                Symbol = other.Symbol;
                Rate = other.Rate;
                NextRate = other.NextRate;
            }
        }

        public bool Equals(FundingRate other)
        {
            if (other == null) return false;

            return (FundingTime == other.FundingTime &&
                    NextFundingTime == other.NextFundingTime &&
                    InstType == other.InstType &&
                    Symbol == other.Symbol &&
                    Rate == other.Rate &&
                    NextRate == other.NextRate);
        }
    }
}
