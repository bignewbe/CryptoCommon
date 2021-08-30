using CryptoCommon.DataTypes;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface ISpotClientService
    {
        //ServiceResult<long> GetOpenOrderList();
        //ServiceResult<SpotOrder> UpdateLastTimeForOrder(SpotOrder order);
        ServiceResult<FZOrder> PlaceOrder(FZOrder order);
        ServiceResult<bool> CancelOrder(FZOrder order);
        ServiceResult<bool> CloseOrder(FZOrder order);
        ServiceResult<string> ModifyOrderPrice(string symbol, string orderId, double newPrice);

        ServiceResult<Dictionary<string, SpotBalance>> GetAccountBalance();
        ServiceResult<Dictionary<string, FZOrder>> GetOpenOrders();
        ServiceResult<Dictionary<string, FZOrder>> GetCloseOrders();

        ServiceResult<PageResult<FZOrder>> GetCloseOrdersByPage(int spage, int epage, int numPerPage = 50);
        ServiceResult<PageResult<FZOrder>> GetCloseOrdersByPage(List<int> pages, int numPerPage = 50);
        ServiceResult<PageResult<FZOrder>> GetCloseOrdersBySymbolAndPage(string symbol, List<int> pages, int numPerPage = 50);


        ServiceResult<List<FZOrder>> GetOpenOrderListBySymbol(string symbol);
        ServiceResult<List<FZOrder>> GetCloseOrderListBySymbol(string symbol);

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
        ServiceResult<List<SpotTradeInfo>> GetAllTraderInfo();
        //ServiceResult<GivenIdConfig> GetGivenIdConfig();
        ServiceResult<List<Dictionary<string, string>>> GetGivenIdConfig();
    }
}
