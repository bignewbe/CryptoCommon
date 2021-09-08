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
        private IFZTrade _trade;

        public OrderProxy(IFZTrade trade, IFZOrder order, List<string> symbols, string dumpfile, int timerInterval, bool isSimuationMode, bool isEnableLog) :
            base(order, symbols, dumpfile, timerInterval, isSimuationMode, isEnableLog)
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

        public FZOrder PlaceOrder(FZOrder order)
        {
            this.LogDebug($"PlaceOrder: {ConvertOrderToStr(order)}");
            var r = _trade.PlaceOrder(order).Result;
            this.AddRefId(order);
            if (!_isSimulationMode) Thread.Sleep(50);
            return r;
        }

        public bool CancelOrder(FZOrder order)
        {
            this.LogDebug($"CancelOrder: {ConvertOrderToStr(order)}");
            var r = _trade.CancelOrder(order.Symbol, order.OrderId).Result;
            if (!_isSimulationMode) Thread.Sleep(50);
            this.AddRefId(order);
            return r;
        }

        public string ModifyOrderPrice(string symbol, string orderId, double newPrice)
        {
            this.LogDebug($"ModifyOrderPrice: {symbol} orderId = {orderId} newPrice = {newPrice}");
            var r = _trade.ModifyOrderPrice(symbol, orderId, newPrice).Result;
            if (!_isSimulationMode) Thread.Sleep(50);
            return r;
        }

        public List<FZOrder> GetOpenOrdersBySymbol(string symbol)
        {
            var orders = OpenOrders.Values.Where(o => o.Symbol == symbol).ToList();
            return orders;
        }

        public List<FZOrder> GetClosedOrdersBySymbol(string symbol)
        {
            var orders = ClosedOrders.Values.Where(o => o.Symbol == symbol).ToList();
            return orders;
        }

        public List<FZOrder> GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        public List<FZOrder> GetClosedOrders()
        {
            throw new NotImplementedException();
        }
    }
}
