using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Interfaces
{
    public interface IRateGate
    {
        bool WaitToProceed(int millisecondsTimeout);
        bool WaitToProceed(TimeSpan timeout);
        void WaitToProceed();
    }
}
