using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon
{
    public enum EnumType
    {
        None,
        SpotTrader3,
        SpotSwingBand,
        SpotSwingShoot,
        FutureTakeProfit,
        SpotGrid,
        SpotBandGrid,
        
        match_match = 10,
        match_bid,
        bid_bid,
        ask_ask,
        ask_bid,
        bid_ask,

        SpotSwingBidAsk,
        BuyFirst1,
        BuyFirst2,
        SellFirst1,
        SellFirst2,
        SellFirst3,
        BuyFirst3,
        Rsi_singleUndershoot,
        Rsi_doubleBottom,
        Rsi_singleOvershoot,
        Rsi_doubleTop
    }
}
