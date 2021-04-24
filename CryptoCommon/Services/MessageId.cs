namespace CryptoCommon.Services
{
    public enum MessageId
    {
        /// 1. {exch}_ICrpytoQuoteService
        Okex_Spot_QuoteBasicDataAppended,
        Okex_Spot_OnTickerList,
        Okex_Spot_OnCandleList,
        Okex_Spot_OnSpotOrderList,
        Okex_Spot_OnSpotBalance,
        Okex_Spot_OnSpotBalanceList,
        Okex_Spot_OnOrderBook,

        Okex_Spot_OnSpotTradeInfo,
        Okex_Spot_OnOrderUpdated,
        Okex_Spot_OnOpenOrderList,
        Okex_Spot_OnCloseOrderList,

        Okex_Future_QuoteBasicDataAppended,
        Okex_Future_OnTickerList,
        Okex_Future_OnCandleList,
        Okex_Future_OnFutureOrderList,
        Okex_Future_OnAccountPosition,
        Okex_Future_OnAccountBalance,
        Okex_Future_OnOrderBook,

        //SwapConOrder
        Okex_Future_OnOpenSwapConOrderList,
        Okex_Future_OnClosedSwapConOrderList,
        Okex_Future_OnSwapConOrderChanged,
        Okex_Future_OnFutureTradeStatus,

        
        Okex_Swap_QuoteBasicDataAppended,
        Okex_Swap_OnTickerList,
        Okex_Swap_OnCandleList,
        Okex_Swap_OnFutureOrderList,
        Okex_Swap_OnAccountPosition,
        Okex_Swap_OnAccountBalance,
        Okex_Swap_OnOrderBook,

        Kraken_QuoteBasicDataAppended,
        Kraken_OnTickerList,
        Kraken_OnCandleList,

        Bittrex_QuoteBasicDataAppended,
        Bittrex_OnTickerList,
        Bittrex_OnCandleList
    }
}
