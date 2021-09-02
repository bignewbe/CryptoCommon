using CryptoCommon.DataTypes;
using CryptoCommon.Services;
using PortableCSharpLib;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public class OrderProxy : OrderProxyBase, IOrderProxy
    {
        private ITradeService _trade;
        private ConcurrentDictionary<string, SyncStatus> _syncOrderAction = new ConcurrentDictionary<string, SyncStatus>();
        private long _currentTime;

        public OrderProxy(ITradeService trade, List<string> symbols, string dumpfile, int timerInterval, bool isSimuationMode, bool isEnableLog) :
            base(trade, symbols, dumpfile, timerInterval, isSimuationMode, isEnableLog)
        {
            _trade = trade;
        }

        public bool IsOrderActionInProgress(string symbol)
        {
            if (!_syncOrderAction.ContainsKey(symbol)) return false;
            return _syncOrderAction[symbol].IsHandlingInProgress(this.GetCurrentTime().GetUnixTimeFromUTC());
        }

        public void SubmitOrder(CommandId commandId, FZOrder order)
        {
            if (commandId == CommandId.PlaceOrder)
                this.PlaceOrder(order);
            else if (commandId == CommandId.CancelOrder)
                this.CancelOrder(order);
        }

        public void PlaceOrder(FZOrder order)
        {
            this.LogDebug($"PlaceOrder: {ConvertOrderToStr(order)}");
            var r = _trade.PlaceOrder(order);
            this.AddRefId(order);
            if (!_isSimulationMode) Thread.Sleep(50);
        }

        public void CancelOrder(FZOrder order)
        {
            this.LogDebug($"CancelOrder: {ConvertOrderToStr(order)}");
            var isStopOrder = order.Ordertype == OrderType.stop_buy || order.Ordertype == OrderType.stop_sell;
            var orderId = isStopOrder ? order.AlgoId : order.OrderId;
            var r = _trade.CancelOrder(order.Symbol, orderId, isStopOrder);
            if (!_isSimulationMode) Thread.Sleep(50);
            this.AddRefId(order);
        }

        public void ModifyOrderPrice(string symbol, string orderId, double newPrice)
        {
            this.LogDebug($"ModifyOrderPrice: {symbol} orderId = {orderId} newPrice = {newPrice}");
            var r = _trade.ModifyOrderPrice(symbol, orderId, newPrice);
            if (!_isSimulationMode) Thread.Sleep(50);
        }

        public void UpdateTime(long time)
        {
            _currentTime = time;
        }

        public DateTime GetCurrentTime()
        {
            return _isSimulationMode ? _currentTime.GetUTCFromUnixTime() : DateTime.UtcNow;
        }

        private void AddRefId(FZOrder order)
        {
            if (!_syncOrderAction.ContainsKey(order.Symbol))
            {
                Func<long> fun = () => this.GetCurrentTime().GetUnixTimeFromUTC();
                var s = new SyncStatus(300, fun);
                _syncOrderAction.TryAdd(order.Symbol, s);
            }
            _syncOrderAction[order.Symbol].AddRefId(order.RefId);
        }

        private void RemoveRefId(FZOrder order)
        {
            if (_syncOrderAction.ContainsKey(order.Symbol))
                _syncOrderAction[order.Symbol].RemoveRefId(order.RefId);
        }
    }
}
