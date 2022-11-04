using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public class TpOrderState
    {
        public string orderId { get; set; }
        public string side { get; set; }
        public string qty { get; set; }
        public string ccy { get; set; }
        public string symbol { get; set; }
        public string oh { get; set; }
        public string ou { get; set; }
        public string ch { get; set; }
        public string cu { get; set; }
        public string openPrice { get; set; }
        public string closePrice { get; set; }
        public string openPeriod { get; set; }
        public string closePeriod { get; set; }
        public string openEnabled { get; set; }
        public string openFinalized { get; set; }
        public string openFilled { get; set; }
        public string closeEnabled { get; set; }
        public string closeFinalized { get; set; }
        public string closeFilled { get; set; }
        public string orderFinalized { get; set; }
        public string started { get; set; }
        public string closed { get; set; }
        public string timeCreated { get; set; }
        public string timeLast { get; set; }
        public string rate1 { get; set; }
        public string rate2 { get; set; }
        public bool IsOrderOpen { get; set; }
    }
}
