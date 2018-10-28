using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    class QuoteCaptureFileInfo
    {
        public string Symbol { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string FileName { get; set; }
        public string Folder { get; set; }
        public string FullFileName { get { return Path.Combine(Folder, FileName); } }

        public QuoteCaptureFileInfo()
        {
        }
        public QuoteCaptureFileInfo(string symbol, long stime, long etime, string fullFilename)
        {
            this.Symbol = symbol;
            this.StartTime = stime;
            this.EndTime = etime;
            this.Folder = Path.GetDirectoryName(fullFilename);
            this.FileName = Path.GetFileName(fullFilename);
        }
    }
}
