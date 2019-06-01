using CryptoCommon.DataTypes;

namespace CryptoCommon.ClientInterface
{
    public interface IAssetClient
    {
        Assets GetAssets();
        string Withdraw(string currency, double amount, string address);
    }
}
