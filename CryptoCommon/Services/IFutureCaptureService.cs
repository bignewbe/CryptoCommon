using CryptoCommon.DataTypes;
using CryptoCommon.Future.Interface;
using System.Collections.Generic;

namespace CryptoCommon.Services
{
    public interface IFutureCaptureService : ICryptoCaptureService
    {
        ServiceResult<List<PublicTrade>> GetPublicTradesByInstrumentId(string instrument_id); //平台成交
        ServiceResult<PublicHoldAmount> GetPublicOpenInterestByInstrumentId(string instrument_id);  //平台持仓
        ServiceResult<List<ForcedLiquid>> GetPubicLiquidationByInstrumentId(string instrument_id); //平台爆仓
    }
}
