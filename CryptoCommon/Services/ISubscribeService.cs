using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.Services
{
    public interface ISubscribeService
    {
        //void Subscribe(List<string> symbols);
        ServiceResult<bool> Subscribe(SubscribeType subscribeType, List<string> symbolOrCurrencies);
        ServiceResult<bool> Unbscribe(SubscribeType subscribeType, List<string> symbolOrCurrencies);
    }
}
