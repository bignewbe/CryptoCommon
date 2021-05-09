using CryptoCommon.DataTypes;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface ISpotClientService
    {
        //ServiceResult<long> GetOpenOrderList();
        //ServiceResult<SpotOrder> UpdateLastTimeForOrder(SpotOrder order);
        ServiceResult<SpotOrder> PlaceOrder(SpotOrder order);
        ServiceResult<bool> CancelOrder(SpotOrder order);

        ServiceResult<Dictionary<string, SpotBalance>> GetAccountBalance();
        ServiceResult<Dictionary<string, SpotOrder>> GetOpenOrderList();
        ServiceResult<Dictionary<string, SpotOrder>> GetCloseOrderList();
        ServiceResult<List<SpotOrder>> GetCloseOrdersByPage(int spage, int epage, int numPerPage = 50);
        ServiceResult<List<SpotOrder>> GetCloseOrdersByPage(List<int> pages, int numPerPage = 50);
        ServiceResult<List<SpotOrder>> GetOpenOrderListBySymbol(string symbol);
        ServiceResult<List<SpotOrder>> GetCloseOrderListBySymbol(string symbol);
        ServiceResult<List<SpotOrder>> GetCloseOrdersBySymbolAndPage(string symbol, List<int> pages, int numPerPage = 50);

        ServiceResult<List<SwingParam>> GetTraderParam();
        ServiceResult<List<string>> GetSpotMonitorSymbols();

        ServiceResult<bool> Subscribe();
        ServiceResult<bool> StartAllTraders();
        ServiceResult<bool> StopAllTraders();
        ServiceResult<bool> StartTraderByParamId(string paramId);
        ServiceResult<bool> StopTraderByParamId(string paramId);
        ServiceResult<Dictionary<string, bool>> GetTraderStatus();

        ServiceResult<SpotTradeInfo> GetSpotTradeInfo(string symbol, string paramId);
        ServiceResult<SpotTradeInfo> GetTraderInfoById(string paramId);
    }
}
