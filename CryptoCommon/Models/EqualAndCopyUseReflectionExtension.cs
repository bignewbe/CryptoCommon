using CryptoCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Models
{
    public static class EqualAndCopyUseReflectionExtension
    {
        public new static bool Equals(this object obj, object other)
        {
            if (other == null) return false;
            var properties = obj.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            foreach (var p in properties)
            {
                var v1 = p.GetValue(obj);
                var v2 = p.GetValue(other);
                if (v1 == null && v2 == null) continue;
                if (v1 == null || v2 == null) return false;
                if (!v1.Equals(v2)) return false;
            }
            return true;
        }

        public static void Copy(this object obj, object other)
        {
            if (other == null) return;
            var properties = obj.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
            foreach (var p in properties)
            {
                var v2 = p.GetValue(other);
                p.SetValue(obj, v2);
            }
        }
    }
}
