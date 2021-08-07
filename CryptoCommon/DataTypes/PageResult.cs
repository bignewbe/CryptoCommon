using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    public class PageResult<T>
    {
        public int TotalPage { get; set; }
        public int NumPerPage { get; set; }
        public int StartPage { get; set; } = -1;
        public int EndPage { get; set; } = -1;
        public List<T> Items { get; set; }
    }
}
