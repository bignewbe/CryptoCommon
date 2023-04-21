using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase.Interface
{
    public interface IOrderDb
    {
        Task AddUpdateAsync(FZOrder order);
        Task DeleteCancelledOrdersAsync();
        Task<FZOrder> GetOrderByOrderId(string orderId);
        Task<List<FZOrder>> GetAllOrdersAsync();
        Task DeleteOrders(params string[] orderIds);
    }
}
