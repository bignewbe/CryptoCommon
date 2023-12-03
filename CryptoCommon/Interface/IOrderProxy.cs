using CryptoCommon.DataTypes;
using Newtonsoft.Json;
using PortableCSharpLib;
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

    public interface IOrderProxy
    {
        HashSet<string> Symbols { get; }

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

        bool IsOrderActionInProgress(string symbol);

        void SetDumpFile(bool v);
        void Start();
        void Stop();
        FZOrder PlaceOrder(FZOrder order);
        bool CancelOrder(FZOrder order);
        FZOrder ModifyOrderSzAndPx(FZOrder order);
        FZOrder ModifyOrderPx(FZOrder order);

        //void ModifyOrderPrice(string symbol, string orderId, double newPrice);
        //FZOrder ModifyOrderSz(FZOrder order);

        //void PlaceOrder(FZOrder order);
        //void CancelOrder(FZOrder order);
        //FZOrder CheckOrder(FZOrder order);

        List<FZOrder> GetOpenOrdersBySymbol(string symbol);
        List<FZOrder> GetClosedOrdersBySymbol(string symbol);

        List<FZOrder> GetOpenOrders();
        List<FZOrder> GetClosedOrders();

        void UpdateOrders(params FZOrder[] orders);
        Orderbook GetOrderbook(string symbol);
        //double GetMinSz(string symbol);
    }
}
