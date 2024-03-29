﻿using CryptoCommon.DataTypes;
using System.Collections.Generic;

namespace CryptoCommon.Future.Interface
{
    //public class AccountHolding
    //{
    //    public Dictionary<string, AccountHoldingPerCrypto> info { get; set; }
    //}

    public class AccountHoldingPerCrypto
    {
        public double Equity { get { return FZOrder.ConvertStrToDouble(equity); } }
        public double Total_avail_balance { get { return FZOrder.ConvertStrToDouble(total_avail_balance); } }

        public string auto_margin { get; set; }
        public List<AccountContract> contracts { get; set; }
        public string liqui_mode { get; set; }
        public string margin_mode { get; set; }
        public string equity { get; set; }
        public string total_avail_balance { get; set; }
    }

    public class AccountContract
    {
        public double Available_qty { get { return FZOrder.ConvertStrToDouble(available_qty); } }
        public double Fixed_balance { get { return FZOrder.ConvertStrToDouble(fixed_balance); } }
        public double Margin_for_unfilled { get { return FZOrder.ConvertStrToDouble(margin_for_unfilled); } }
        public double Margin_frozen { get { return FZOrder.ConvertStrToDouble(margin_frozen); } }
        public double Realized_pnl { get { return FZOrder.ConvertStrToDouble(realized_pnl); } }
        public double Unrealized_pnl { get { return FZOrder.ConvertStrToDouble(unrealized_pnl); } }

        public string available_qty { get; set; }
        public string fixed_balance { get; set; }
        public string instrument_id { get; set; }
        public string margin_for_unfilled { get; set; }
        public string margin_frozen { get; set; }
        public string realized_pnl { get; set; }
        public string unrealized_pnl { get; set; }
    }
}
