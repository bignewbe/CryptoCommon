namespace CryptoCommon.Services
{
    public enum ServiceId
    {
        Okex_ISubscribeService,

        Okex_Spot_ICrpytoQuoteService,
        Okex_Spot_ICryptoCaptureService,
        Okex_ISpotTradeService,
        Okex_ISpotClientService,

        Okex_Future_ICrpytoQuoteService,
        Okex_IFutureCaptureService,
        Okex_IFutureTradeService,
        Okex_IFutureTakeProfitService,

        Okex_Swap_ICrpytoQuoteService,
        Okex_ISwapCaptureService,
        Okex_ISwapTradeService,

        Kraken_ICrpytoQuoteService,
        Kraken_ICryptoCaptureService,
        Bittrex_ICrpytoQuoteService,
        Bittrex_ICryptoCaptureService,
    }
}
