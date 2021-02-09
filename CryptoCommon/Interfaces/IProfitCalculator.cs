namespace CryptoCommon.Interfaces
{
    public interface IProfitCalculator
    {
        event CryptoCommon.EventHandlers.OneWayCoinToCoinProfitCalculatedEventHandler OnOneWayCoinToCoinProfitCalculated;
        event CryptoCommon.EventHandlers.TwoWayCoinToCoinProfitCalculatedEventHandler OnTwoWayCoinToCoinProfitCalculated;
        event CryptoCommon.EventHandlers.OneWayCoinToCurrencyProfitCalculatedEventHandler OnOneWayCoinToCurrencyProfitCalculated;
        event CryptoCommon.EventHandlers.TwoWayCoinToCurrencyProfitCalculatedEventHandler OnTwoWayCoinToCurrencyProfitCalculated;
    }
}