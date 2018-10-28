namespace CryptoCommon.Interfaces
{
    public interface ICache<T>
    {
        int MaxCount { get; }
        T GetItem(string key);
        bool RemoveItem(string key);
        bool AddItem(string key, T quote);
    }
}
