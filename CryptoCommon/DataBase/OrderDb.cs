using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataBase.Interface;
using CryptoCommon.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase
{
    public class OrderDb : IOrderDb
    {
        private ConcurrentDictionary<string, int> _orderIdToId = new ConcurrentDictionary<string, int>();
        private IRepo<FZOrderEnt> _repo;

        public OrderDb(IRepo<FZOrderEnt> repo)
        {
            _repo = repo;
        }

        public async Task AddUpdateAsync(FZOrder order)
        {
            var orderId = order.OrderId;
            if (_orderIdToId.ContainsKey(orderId))
            {
                var et = new FZOrderEnt(order) { Id = _orderIdToId[orderId] };
                //await _repo.UpdateAsync(et).ConfigureAwait(false);
                await _repo.UpdateByIdAsync(_orderIdToId[orderId], updates => updates.SetProperty(p => p.Order, p => order)).ConfigureAwait(false);
            }
            else
            {
                var t = await _repo.GetByNameAsync(orderId).ConfigureAwait(false);
                if (t != null)
                {
                    //t.Order = order;
                    //await _repo.UpdateAsync(t).ConfigureAwait(false);
                    _orderIdToId.TryAdd(orderId, t.Id);
                    await _repo.UpdateByIdAsync(_orderIdToId[orderId], updates => updates.SetProperty(p => p.Order, p => order)).ConfigureAwait(false);
                }
                else
                {
                    var et = new FZOrderEnt(order);
                    await _repo.AddAsync(et).ConfigureAwait(false);
                }
            }
        }

        public async Task<List<FZOrder>> GetAllOrdersAsync()
        {
            var lst = await _repo.GetAllAsync().ConfigureAwait(false);
            foreach (var o in lst)
            {
                if (o.Id <= 0)
                    Console.WriteLine();
                if (!_orderIdToId.ContainsKey(o.Name))
                    _orderIdToId.TryAdd(o.Name, o.Id);
            }
            return lst.Select(t => t.Order).ToList();
        }

        public async Task DeleteCancelledOrdersAsync()
        {
            var lst = await _repo.GetAllAsync().ConfigureAwait(false);
            var ids = lst.Where(t => t.Order.State == OrderState.cancelled).Select(t => t.Id).ToArray();
            if (ids.Length > 0)
            {
                await _repo.DeleteByIdsAsync(ids).ConfigureAwait(false);
                foreach (var t in lst)
                    if (ids.Contains(t.Id))
                        _orderIdToId.TryRemove(t.Name, out _);
            }
        }

        public async Task DeleteOrders(params string[] orderIds)
        {
            if (orderIds.Length > 0)
            {
                await _repo.DeleteByNamesAsync(orderIds).ConfigureAwait(false);
                foreach (var orderId in orderIds)
                    _orderIdToId.TryRemove(orderId, out _);
            }
        }

        public async Task<FZOrder> GetOrderByOrderId(string orderId)
        {
            var o = await _repo.GetByNameAsync(orderId).ConfigureAwait(false);
            if (o != null)
                _orderIdToId.TryAdd(o.Name, o.Id);
            return o.Order;
        }
    }
}
