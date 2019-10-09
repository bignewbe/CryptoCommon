namespace CryptoCommon.Future.Interface
{
    public enum FutureState
    {
        None,
        Open,
        Failed,
        Cancelled,
        PartialFilled,
        FullyFilled,
        Summitting,
        Cancelling
    }
}
