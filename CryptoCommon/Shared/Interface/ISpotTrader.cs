using CryptoCommon.DataTypes;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface ISpotTrader
    {
        string RefIdStr { get; }
        string Exchange { get; }
        string Symbol { get; }
        
        SwingParam Param { get; }
        SpotTradeInfo Status { get; }

        bool IsStarted { get; }

        void Start();
        void Stop();

        //List<FZOrder> GetOpenOrders();
        //event EventHandlers.HoldPositionChangedEventHandler OnHoldPositionChanged;
        //event EventHandlers.AleartDetectedEventHandler OnAlertDetected;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotTradeInfo> OnProxyStatusChanged;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<string> OnPerformTradingCalled;

        //event EventHandlers.QtyFutureOpenedClosedChangedEventHandler OnFutureOpenedClosed;
        //void CheckOrderSubmit();
        //void LoadParam();
        //void PerformTrading(int pass=0);
        void PerformTrading();
        //void SetTraderState(bool isEnabled);
        void AddAdjustmentOrder(string symbol, OrderType t, OrderState s, double price, double qty);
        void RemoveAdjustmentOrder(string symbol, string refIdStr);
        List<FZOrder> SubmitOrders(OrderType type, double quantity, double min, double max, int numTrades, string refIdPre = null);
        bool CancelOverTimeOrders(OrderType orderType, long utcNow, double allowTimeInSeconds);
        SpotTradeInfo GetTraderInfo(long? startTime = null);

        string CreateRefId();

        (double netc1, double netc2, double qtyBought, double avgBoughtPrice, double qtySold, double avgSoldPrice, int numOrders, double minC1, double minC2, double avgHoldTime) 
            CreateReport(string folder=null, long? stime=null, long? etime=null, double? currentPrice = null, bool isWriteFile = true);
    }
}
