using System;
using System.Collections.Generic;

namespace CryptoCommon.DataTypes
{
    public class Assets : IEquatable<Assets>
    {
        public string Exchange { get; set; }
        public Dictionary<string, double> Free { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Freezed { get; set; } = new Dictionary<string, double>();

        public Assets()
        {
        }

        public Assets(Assets assets)
        {
            this.Copy(assets);
        }

        public bool Equals(Assets other)
        {
            if (Free == null && other.Free == null) return true;
            if (Free == null || other.Free == null) return false;
            
            if (Exchange != other.Exchange) return false;            
            return Free.DictionaryEqual(other.Free);

            //if (Freezed == null || other.Freezed != null) return false;
            //if (Freezed != null && !Freezed.DictionaryEqual(other.Freezed)) return false;
            //return true;
        }

        public void Copy(Assets other)
        {
            if (other == null) return;

            this.Exchange = other.Exchange;            
            this.Free = other.Free==null? null : new Dictionary<string, double>(other.Free);
            this.Freezed = other.Freezed == null ? null : new Dictionary<string, double>(other.Freezed);
        }
    }
}
