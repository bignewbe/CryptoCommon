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

    public class FZOrderEnt2 : FZOrder, IIdAndName<FZOrderEnt2> 
    {
        public new int Id { get; set; }
        public string Name { get; set; }

        public FZOrderEnt2() {}
        public FZOrderEnt2(FZOrder order) => this.Copy(order);
        public FZOrderEnt2(FZOrderEnt2 order) => this.Copy(order);

        public void Copy(FZOrder other)
        {
            if (other != null)
            {
                Name = other.OrderId;
                base.Copy(other);                
            }
        }

        public void Copy(FZOrderEnt2 other)
        {
            this.Copy(other);
            this.Name = other.Name;
        }

        public bool Equals(FZOrderEnt2? other)
        {
            return other!=null && base.Equals(other);
        }
    }
}
