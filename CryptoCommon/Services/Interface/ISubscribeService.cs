using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.Services
{
    public interface ISubscribeService
    {
        //ServiceResult<bool> SubscribeAllSymbols(SubscribeType subscribeType);
        ServiceResult<bool> Subscribe(SubscribeType subscribeType, List<string> symbolOrCurrencies);
        ServiceResult<bool> Unbscribe(SubscribeType subscribeType, List<string> symbolOrCurrencies);
    }
}
