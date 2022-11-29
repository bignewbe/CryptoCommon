using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase.Interface
{
    public interface IIdAndName<T> : IEquatable<T> 
    {
        int Id { get; set; }
        string Name { get; set; }
        void Copy(T other);
    }
}
