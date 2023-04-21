using CryptoCommon.DataBase.Interface;
using CryptoCommon.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase.Entity
{
    public class FZOrderEnt : IIdAndName<FZOrderEnt>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public FZOrder Order { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public FZOrderEnt()
        {
        }
        public FZOrderEnt(FZOrder order)
        {
            if (order == null) return;
            Name = order.OrderId;
            Order = order;
        }

        public void Copy(FZOrderEnt other)
        {
            if (other != null)
            {
                Id = other.Id;
                Name = other.Name;
                Order = other.Order;
            }
        }

        public bool Equals(FZOrderEnt other)
        {
            if (other == null) return false;
            return Id == other.Id && Name == other.Name && Order.Equals(other.Order);
        }
    }

}
