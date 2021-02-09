using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.Future.DataType
{
    public enum ExecutionType
    {
        None,
        Standard,
        UseMatchPrice,
        FillOrKill,
        ImmediateAndCancel,
        MakerOnly
    }
}
