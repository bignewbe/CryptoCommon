using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public class Funding
    {
        public DateTime CreateDate { get; set; }
        public string Currency { get; set; }
        public string Exchange { get; set; }
        public FundingType FundingType { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string Method { get; set; }
        public double Amount { get; set; }
        public double Fee { get; set; }
        public FundingStatus Status { get; set; }
        public string RefId { get; set; }
        public string txid { get; set; }
    }
}
