using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.Interfaces
{
    public interface IIdEqualCopy<T> : IEquatable<T> where T : class
    {
        string Id { get; }
        void Copy(T other);
    }
}
