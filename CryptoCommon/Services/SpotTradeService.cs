using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using System;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public class SpotTradeService : ISpotTradeService
    {
        IOrder _apiOrder;
        IFund _fund;

        public SpotTradeService(IOrder om, IFund fm)
        {
            _apiOrder = om;
            _fund = fm;
        }

        //public ServiceResult<bool> UpdateLastTimeForOrder(SpotOrder order)
        //{
        //    return ServiceResult<bool>.CallAsyncFunction(() => _apiOrder.UpdateLastTimeForOrder(order)).Result;
        //}

        public ServiceResult<long> GetLastTimeForOrder(string orderId, string symbol)
        {
            try
            {
                return ServiceResult<long>.CallAsyncFunction(() => _apiOrder.GetLastTimeForClosedOrder(orderId, symbol)).Result;
            }
            catch (Exception e)
            {
                return new ServiceResult<long> { Result = false, Message = e.ToString() };
            }
        }

        public ServiceResult<bool> CancelOrder(string symbol, string orderId, bool isStopOrder)
        {
            return ServiceResult<bool>.CallAsyncFunction(() => _apiOrder.CancelOrder(symbol, orderId, isStopOrder)).Result;
        }

        public ServiceResult<SpotOrder> CheckOrder(string symbol, string orderId, bool isStopOrder = false)
        {
            try
            {
                //var order = new SpotOrder { Exchange = _apiOrder.Exchange, Symbol = symbol, OrderId = orderId };
                return ServiceResult<SpotOrder>.CallAsyncFunction(() => _apiOrder.CheckOrder(symbol, orderId, isStopOrder)).Result;
            }
            catch (Exception e)
            {
                return new ServiceResult<SpotOrder> { Result = false, Message = e.ToString() };
            }
        }

        public ServiceResult<Dictionary<string, SpotBalance>> GetAccountBalance()
        {
            try
            {
                var assets = _fund.GetAssets().Result;
                var balances = new Dictionary<string, SpotBalance>();
                foreach (var c in assets.Free.Keys)
                {
                    var free = assets.Free[c];
                    var freezed = assets.Freezed.ContainsKey(c) ? assets.Freezed[c] : 0;
                    var id = assets.Ids[c];
                    var b = new SpotBalance
                    {
                        Currency = c,
                        Available = free,
                        Hold = freezed
                    };
                    balances.Add(c, b);
                }
                return new ServiceResult<Dictionary<string, SpotBalance>> { Result = true, Data = balances };
            }
            catch (Exception e)
            {
                return new ServiceResult<Dictionary<string, SpotBalance>> { Result = false, Message = e.ToString() };
            }
        }

        public ServiceResult<List<SpotOrder>> GetAllOpenOrders()
        {
            return ServiceResult<List<SpotOrder>>.CallAsyncFunction(() => _apiOrder.GetAllOpenOrders()).Result;
        }

        public ServiceResult<List<SpotOrder>> GetClosedOrdersBySymbol(string instrument_id, bool isReturnAll = false)
        {
            return ServiceResult<List<SpotOrder>>.CallAsyncFunction(() => _apiOrder.GetClosedOrdersBySymbol(instrument_id, isReturnAll)).Result;
        }

        public ServiceResult<List<SpotOrder>> GetOpenOrdersBySymbol(string instrument_id)
        {
            return ServiceResult<List<SpotOrder>>.CallAsyncFunction(() => _apiOrder.GetOpenOrdersBySymbol(instrument_id)).Result;
        }

        public ServiceResult<SpotOrder> PlaceOrder(SpotOrder order)
        {
            return ServiceResult<SpotOrder>.CallAsyncFunction(() => _apiOrder.PlaceOrder(order)).Result;
        }

        public ServiceResult<List<SpotOrder>> GetOpenStopOrder(string symbol)
        {
            return ServiceResult<List<SpotOrder>>.CallAsyncFunction(() => _apiOrder.GetOpenStopOrder(symbol)).Result;
        }

        //public ServiceResult<SpotOrder> PlaceStopOrder(string symbol, OrderType type, double amount, double tpPrice, double slPrice)
        //{
        //    var order = new SpotOrder { Symbol = symbol, Ordertype = type, Amount = amount, TPTriggerPrice=tpPrice, TPPrice=tpPrice, SLTriggerPrice=slPrice, SLPrice=slPrice};
        //    return ServiceResult<SpotOrder>.CallAsyncFunction(() => _apiOrder.PlaceOrder(order)).Result;
        //}

        //public ServiceResult<SpotOrder> PlaceOrder(string symbol, OrderType type, double price, double amount, string refid, double? triggerprice = null)
        //{
        //    var order = new SpotOrder { Symbol = symbol, Ordertype = type, Price = price, Amount = amount, RefId = refid };
        //    if (triggerprice.HasValue)
        //        order.TriggerPrice = triggerprice.Value;
        //    return ServiceResult<SpotOrder>.CallAsyncFunction(() => _apiOrder.PlaceOrder(order)).Result;
        //}
    }
}
