using CommonCSharpLibary.TechnicalAnalysis;
using CryptoModels.Models;
using System.Collections.Generic;

namespace CryptoModels.Interfaces
{
    public interface ITraderModel //required quotebasics and indicators are given via constructor
    {
        void CheckRemoveMonitorOrder(string symbol, double high, double low);
        BidOrder CreateNewOrder(int eindex, params CSelectParams[] selectParams);
        void UpdateStop(long time, double close, HoldPosition order);
        List<KeyValuePair<int, double>> CalculateListStopPrice(SoldPosition sold, int num);
    }
}