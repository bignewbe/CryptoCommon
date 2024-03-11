using CryptoCommon.DataTypes;
using CryptoCommon.Shared.ExchProxy;
using Newtonsoft.Json;
using PortableCSharpLib;
using PortableCSharpLib.DataType;
using PortableCSharpLib.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Services
{
    public class PotentialOrder
    {
        public DateTime TimeCreatedStr => TimeCreated.GetUTCFromUnixTime();
        public string Side => IsBuyFirst ? "buy" : "sell";
        public string Id => $"{ParamId}_{Symbol}_{Interval}_{Side}_{TimeCreatedStr}";
                
        public string ParamId { get; set; }
        public string Symbol { get; set; }
        public int Interval { get; set; }
        public long TimeCreated { get; set; }
        public List<double> Prices { get; set; }
        public double QtyEach { get; set; }
        public bool IsBuyFirst { get; set; } 
        public bool IsPriceAdjusted { get; set; } = false;
        public bool IsSubmitted { get; set; } = false;
        public bool IsExpired { get; set; } = false;
        public double StartPrice { get; set; }
        public double EndPrice { get; set; }
        public double PrevPrice { get; set; }

        public PotentialOrder(string paramId)
        {
            this.ParamId = paramId;
        }
    }

    public interface IExchProxy
    {
        string Exchange { get; }
        HashSet<string> Symbols { get; }
        IGenericProxy<AccountPosition> PositionProxy { get; }
        IGenericProxy<AccountBalance> BalanceProxy { get; }

        ConcurrentDictionary<string, PotentialOrder> PendingOrders { get; }
        void AddPendingOrder(PotentialOrder o);
        void RemovePendingOrder(string paramId);
        bool MoveBackPendingOrder(string paramId, int lookbackSeconds = 14400);
        bool IsPendingOrderExist(string paramId);
        int GetNumOfPastOrders(int seconds, long tnow);
        List<long> WaterfallTimeStamps { get; }

        //ConcurrentDictionary<string, long> PlaceOrderTimeBySymbolBuyFirst { get; }
        //ConcurrentDictionary<string, long> PlaceOrderTimeBySymbolSellFirst { get; }
        long GetPlaceOrderTimeGlobal(bool isBuyFirst);
        void SetPlaceOrderTime(string symbol, long tnow, bool isBuyFirst);
        long GetPlaceOrderTime(string symbol, bool isBuyFirst);
        void SetPrevPrice(string symbol, double price, bool isBuyFirst);
        double GetPrevPrice(string symbol, bool isBuyFirst);
        (int countExecuted, int countCreated, double thd) ComputeUFR(List<FZOrder> openOrders);
        long GetLastUfrOrderTime();
        void SetUfrOrderTime(long time);

        bool IsStarted { get; }
        DateTime GetCurrentTime();
        ConcurrentDictionary<string, FZOrder> OpenOrders { get; }
        ConcurrentDictionary<string, FZOrder> ClosedOrders { get; }

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderCancelled;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;

        void SetDumpFile(bool v);
        void Start();
        void Stop();


        double ConvertToExchPrice(string symbol, double price);
        double ApproximateQtyDigits(string symbol, OrderType ot, double qty);

        bool IsOrderActionInProgress(string symbol);
        FZOrder FindParent(string refId);

        FZOrder PlaceOrder(FZOrder order);
        FZOrder PlaceOrder(string symbol, OrderType orderType, double price, double qty, string refId=null);
        FZOrder ModifyOrder(FZOrder order);

        bool CancelOrder(FZOrder order);

        void UpdateOrders(params FZOrder[] orders);
        Orderbook GetOrderbook(string symbol);

        TradeState GetAccountState(string symbol);
        
        //void UpdateFuturePnl(string symbol, double price);
        void UpdateFuturePnl(ConcurrentDictionary<string, OHLC> candleToProcessDic);
        (double walletBalance, double urealizedPnl, double initMargin, double initPositionValue, double maxPnl, string idMaxPnl) GetFuturePnl();

        //List<FZOrder> GetOpenOrders(params OrderType[] orderTypes);
        //List<FZOrder> GetCloseOrders(params OrderType[] orderTypes);
        //List<FZOrder> GetOpenOrdersByRefId(string refId, params OrderType[] orderTypes);
        //List<FZOrder> GetOpenOrdersBySymbol(string symbol, params OrderType[] orderTypes);
        //List<FZOrder> GetCloseOrdersByRefId(string refId, params OrderType[] orderTypes);
        //List<FZOrder> GetCloseOrdersBySymbol(string symbol, params OrderType[] orderTypes);

        //List<FZOrder> GetOpenOrdersBySymbol(string symbol);
        //List<FZOrder> GetClosedOrdersBySymbol(string symbol);
        //List<FZOrder> GetOpenOrders();
        //List<FZOrder> GetClosedOrders();
    }
}

//double GetMinSz(string symbol);
//FZOrder ModifyOrderSzAndPx(FZOrder order);
//FZOrder ModifyOrderPx(FZOrder order);
//void ModifyOrderPrice(string symbol, string orderId, double newPrice);
//FZOrder ModifyOrderSz(FZOrder order);
//void PlaceOrder(FZOrder order);
//void CancelOrder(FZOrder order);
//FZOrder CheckOrder(FZOrder order);
