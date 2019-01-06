using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon
{
    public class EventHandlers
    {
        public delegate void ConnectionChangedEventHandler(object sender, bool isConnected);

        public delegate void DataBarReceivedEventHandler(object sender, string exchange, OHLC ohlc);
        public delegate void DataBarReceivedEventHandlerList(object sender, string exchange, List<OHLC> ohlc);
        public delegate void TickerReceivedEventHandler(object sender, string exchange, Ticker ticker);
        public delegate void TickerReceivedEventHandlerList(object sender, string exchange, List<Ticker> ticker);
        public delegate void CaptureStateChangedEventHandler(object sender, string exchange, bool isStarted);

        public delegate void ExceptionOccuredEventHandler2(object sender, Exception ex);
        public delegate void ExceptionOccuredEventHandler(object sender, string exchange, Exception ex);
        public delegate void SymbolNotFoundEventHandler(object sender, string symbol);

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


        public delegate void StateChangedEventHandler(object sender, bool isStarted);
        public delegate void PriceUpdatedEventHandler(object sender, string exchange, string symbol, Ticker ticker);
        public delegate void TwoWayCoinToCurrencyProfitCalculatedEventHandler(object sender, string exchange1, string crypto1, string exchange2, string crypto2, double profit);
        public delegate void OneWayCoinToCurrencyProfitCalculatedEventHandler(object sender, string exchange1, string exchange2, string crypto, double profit);
        public delegate void OneWayCoinToCoinProfitCalculatedEventHandler(object sender, string exchange1, string exchange2, string symbol, double profitBuyAtExch1, double profitSellAtExch1, Ticker tickExch1, Ticker tickExch2);

        /// <summary>
        /// buy symbol1@exchange1, sell symbol2@exchange1. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exchange1"></param>
        /// <param name="exchange2"></param>
        /// <param name="symbol1"></param>
        /// <param name="symbol2"></param>
        /// <param name="profit"></param>        
        public delegate void TwoWayCoinToCoinProfitCalculatedEventHandler(object sender, string exchange1, string symbol1, string exchange2, string symbol2, double profit);
    }
}
