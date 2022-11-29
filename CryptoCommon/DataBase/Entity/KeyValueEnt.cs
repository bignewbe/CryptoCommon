using CryptoCommon.DataBase.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataBase.Entity
{
    public class KeyValueEnt<T> : IIdAndName<KeyValueEnt<T>> where T : IEquatable<T>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public T Content { get; set; }

        public void Copy(KeyValueEnt<T> other)
        {
            if (other == null) return;
            Id = other.Id;
            Name = other.Name;
            Content = other.Content;
        }

        public bool Equals(KeyValueEnt<T> other)
        {
            if (other == null) return false;
            if (Id != other.Id || Name != other.Name) return false;
            if (Content == null && other.Content != null) return false;
            else if (Content != null && other.Content == null) return false;
            else if (Content == null && other.Content == null) return true;
            else if (Content.Equals(other.Content)) return false;
            else return true;
        }

        //public override bool Equals(KeyValueEnt<T> other)
        //{
        //    if (other == null) return false;
        //    return Id == other.Id && Name == other.Name && Content.Equals(other.Content);
        //}

        //public override int GetHashCode()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
