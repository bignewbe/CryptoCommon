using CryptoCommon.DataTypes;
using CryptoCommon.Future.Interface;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Generic;
using OHLC = PortableCSharpLib.DataType.OHLC;
using Ticker = PortableCSharpLib.DataType.Ticker;

namespace CryptoCommon
{
    public class EventHandlers
    {
        public delegate void ConnectionChangedEventHandler(object sender, bool isConnected);
        public delegate void DataBarReceivedEventHandler(object sender, string exchange, OHLC ohlc);
        public delegate void CandleListReceivedEventHandler(object sender, string exchange, List<OHLC> ohlc);
        public delegate void TickerReceivedEventHandler(object sender, string exchange, Ticker ticker);
        public delegate void TickerReceivedEventHandlerList(object sender, string exchange, List<Ticker> ticker);
        public delegate void CaptureStateChangedEventHandler(object sender, string exchange, bool isStarted);

        public delegate void ExceptionOccuredEventHandler2(object sender, Exception ex);
        public delegate void ExceptionOccuredEventHandler(object sender, string exchange, Exception ex);
        public delegate void SymbolNotFoundEventHandler(object sender, string symbol);

        public delegate void StateChangedEventHandler(object sender, bool isStarted);
        public delegate void TickerUpdatedEventHandler(object sender, string exchange, Ticker oldtick, Ticker newtick);
        public delegate void TwoWayCoinToCurrencyProfitCalculatedEventHandler(object sender, string exchange1, string crypto1, string exchange2, string crypto2, double profit);
        public delegate void OneWayCoinToCurrencyProfitCalculatedEventHandler(object sender, string exchange1, string exchange2, string crypto, double profit);
        public delegate void OneWayCoinToCoinProfitCalculatedEventHandler(object sender, string exchange1, string exchange2, string symbol, double profitBuyAtExch1, double profitSellAtExch1, Ticker tickExch1, Ticker tickExch2);


        public delegate void TwoWayCoinToCoinProfitCalculatedEventHandler(object sender, string exchange1, string symbol1, string exchange2, string symbol2, double profit);

        //from crypto models
        public delegate void OrderSumittedEventHandler(object sender, string symbol, int orderId);
        public delegate void OrderCancelledEventHandler(object sender, string symbol, int orderId);
        public delegate void OrderExecutedEventHandler(object sender, string symbol, int orderId, double executePrice, int quantity);
        public delegate void OrderClosedEventHandler(object sender, string symbol, int orderId, double closePrice, int quantity);
        
        public delegate void QuoteRemovedEventHandler(object sender, IQuoteBasicBase quote);
        public delegate void QuoteDataReceivedEventHandler(object sender, string symbol, long time, double open, double close, double high, double low, double volume);
        public delegate void EmaUpdatedEventHandler(object sender, List<double> ema, int count);

        public delegate void ChartDataRangeChangedEventHandler(object sender);
        public delegate void DepositAddedEventHandler(object sender, Funding deposit);
        public delegate void DepositStatusChangedEventHandler(object sender, Funding deposit);

        public delegate void WithdrawAddeddEventHandler(object sender, Funding withdraw);
        public delegate void WithdrawStatusChangedEventHandler(object sender, Funding withdraw);
        public delegate void NewOrderAddedEventHandler(object sender, SpotOrder order);

        public delegate void OrderStatusChangedEventHandler(object sender, SpotOrder order);
        public delegate void TwowayCoinToCurrencyOrderChangedEventHandler(object sender, TwowayCoinToCurrencyOrder order);
        public delegate void OnewayCoinToCoinOrderChangedEventHandler(object sender, OnewayCoinToCoinOrder order);

        public delegate void OrderListChangedEventHandler(object sender, IList<SpotOrder> order);
        public delegate void TakeProfitOrderListChangedEventHandler(object sender, IList<TwowayCoinToCurrencyOrder> order);

        public delegate void ExchangeAssetsUpdatedEventHandler(object sender, Assets oldAsset, Assets newAsset);
        public delegate void AssetUpdatedEventHandler(object sender, string exchange, string asset, double oldValue, double newValue);

        public delegate void FreeAssetChangedEventHandler(object sender, string currency, Assets asset);
        public delegate void FreezedAssetChangedEventHandler(object sender, string currency, Assets asset);



        //public delegate void QuoteBasicDataAddedOrUpdatedEventHandler(object sender, string exchange, IQuoteBasicBase quote, int numAppended);
        //public delegate void QuoteCaptureDataAddedOrUpdatedEventHandler(object sender, string exchange, IQuoteCapture quote, int numAppended);
        //public delegate void QuoteSavedEventHandler(object sender, string exchange, string filename);
        //public delegate void DepositAddedEventHandler(object sender, Funding deposit);
        //public delegate void DepositStatusChangedEventHandler(object sender, Funding deposit);
        //public delegate void WithdrawAddeddEventHandler(object sender, Funding withdraw);
        //public delegate void WithdrawStatusChangedEventHandler(object sender, Funding withdraw);
        //public delegate void NewOrderAddedEventHandler(object sender, Order order);
        //public delegate void OrderStatusChangedEventHandler(object sender, Order order);
        //public delegate void TwowayCoinToCurrencyOrderChangedEventHandler(object sender, TwowayCoinToCurrencyOrder order);
        //public delegate void OnewayCoinToCoinOrderChangedEventHandler(object sender, OnewayCoinToCoinOrder order);
        //public delegate void OrderListChangedEventHandler(object sender, IList<Order> order);
        //public delegate void TakeProfitOrderListChangedEventHandler(object sender, IList<TwowayCoinToCurrencyOrder> order);
        ////public delegate void AssetObtainedEventHandler(object sender, Assets asset);
        //public delegate void ExchangeAssetsUpdatedEventHandler(object sender, Assets oldAsset, Assets newAsset);
        //public delegate void AssetUpdatedEventHandler(object sender, string exchange, string asset, double oldValue, double newValue);
        //public delegate void FreeAssetChangedEventHandler(object sender, string currency, Assets asset);
        //public delegate void FreezedAssetChangedEventHandler(object sender, string currency, Assets asset);

        public delegate void ItemWithIdChangedEventHandler<T>(object sender, string id, T item);
        public delegate void ItemChangedEventHandler<T>(object sender, T item);
    }
}
