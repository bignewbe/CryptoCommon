using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.DataTypes
{
    public enum ExecutionType
    {
        None,
        Standard,
        UseMatchPrice,
        FillOrKill,
        ImmediateAndCancel,
        MakerOnly
    }

    //public enum TimeInForceType
    //{
    //    NONE,
    //    GTC, 
    //    IOC, 
    //    FOK
    //}

    public enum InstrumentType
    {
        SPOT,
        SWAP,
        FUTURES,
        OPTION,
        MARGIN,
        NONE,
        ANY
    }

    public enum OrderType
    {
        none,

        buy_limit,
        buy_market,
        sell_limit,
        sell_market,

        open_long,
        open_short,
        close_long,
        close_short,

        stop_sell,
        stop_buy,

        buy_fok,
        sell_fok,
        buy_ioc,
        sell_ioc,
        buy_oli,
        sell_oli
//fok: Fill-or-kill order
//ioc: Immediate-or-cancel order
//optimal_limit_ioc
    }

    public enum OrderState
    {
        none,
        cancel_pending,
        pending,
        open,
        //closed,
        //partial_closed,
        cancelled,      //closed
        expired,        //closed
        failed,         //closed
        //not_exist,

        //none,
        //open,
        //failed,
        //cancelled,
        partial_filled,   //open
        fully_filled,     //closed 
        summitting,
        cancelling,
        effective, //for stop order
        partial_canceled, //closed
    }

    //public enum FutureState
    //{
    //    none,
    //    open,
    //    failed,
    //    cancelled,
    //    partial_filled,
    //    fully_filled,
    //    summitting,
    //    cancelling
    //}

    //public enum FutureType
    //{
    //    none,
    //    open_long,
    //    open_short,
    //    close_long,
    //    close_short
    //}


    //okCoin: （-3:撤销中;-2:已撤销;-1:失败;0:等待提现;1:提现中;2:已汇出;3:邮箱确认;4:人工审核中5:等待身份认

    //Kraken status:
    //cancel-pending = cancelation requested
    //canceled = canceled
    //cancel-denied = cancelation requested but was denied  => failed
    //return = a return transaction initiated by Kraken; it cannot be canceled => transferred
    //onhold = withdrawal is on hold pending review => confirm_needed
    public enum FundingStatus
    {
        none,
        deposit_failed,
        deposit_wait_confirm,
        deposit_succeed,

        withdraw_cancel_denied,
        withdraw_cancel_pending,
        withdraw_canceled,
        withdraw_failed,          //cancel denied, failed, etc
        withdraw_initial,
        withdraw_pending,         //waitting to be transferred
        withdraw_transferred,     //done
        withdraw_confirm_needed,  //need further confirmation
        withdraw_wait_identity_verification
    }

    public enum FundingType
    {
        none,
        deposit,
        withdraw
    }

    public enum EnumInterval
    {
        one_miniute,
        five_minitue,
        fifteen_minute,
        one_hour,
        day
    }

    public enum SubscribeType
    {
        None,
        Candle,
        Ticker,
        //Order,
        //Position,
        //Balance
        //Account,
        //Trade,
        Depth5,
//        Binance,
//        SpotCandle,
//        SpotTicker,
//        SpotOrder,
//        SpotAccount,
//        SpotDepth5,
//        FutureCandle,
//        FutureTicker,
//        FutureOrder,
//        FutureAccount,
//        FuturePosition,
//        FutureDepth,
//        FutureDepth5,
//        FutureDepth200,
//        FutureTrade,
//        SwapCandle,
//        SwapTicker,
//        SwapOrder,
//        SwapAccount,
//        SwapPosition,
//        SwapDepth,
//        SwapDepth5,
//        SwapDepth200,
//        SwapTrade,
        SwapFundingRate
    }

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
        BuyFirst,
        //BuyFirst1,
        //BuyFirst2,
        SellFirst,
        //SellFirst1,
        //SellFirst2,
        //SellFirst3,
        //BuyFirst3,
        Rsi_singleUndershoot,
        Rsi_doubleBottom,
        Rsi_singleOvershoot,
        Rsi_doubleTop,
        SpotEmaCross,
        TraderShoot,
        TraderTurn,
        TraderGrid,
        TestType
    }
}
