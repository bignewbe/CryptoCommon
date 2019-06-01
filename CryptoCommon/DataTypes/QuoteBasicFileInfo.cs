using PortableCSharpLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.DataTypes
{
    class QuoteBasicFileInfo : QuoteCaptureFileInfo
    {
        public int Interval { get; set; }
        public DateTime StartDate { get { return StartTime.GetUTCFromUnixTime(); } set { } }
        public DateTime EndDate { get { return EndTime.GetUTCFromUnixTime(); } set { } }

        public QuoteBasicFileInfo() : base() { }
        public QuoteBasicFileInfo(string symbol, int interval, long startTime, long endTime, string fullFilename) : base(symbol, startTime, endTime, fullFilename)
        {
            Interval = interval;
        }

        //public QuoteFile(string symbol, int interval, long startTime, long endTime, string folder, string filename)
        //{
        //    Symbol = symbol;
        //    Interval = interval;
        //    StartTime = startTime;
        //    EndTime = endTime;
        //    Folder = folder;
        //    //FileName = filename ?? TACommon.GetWeeklyQuoteFileName(Symbol, Interval, EndDate);
        //    FileName = filename;
        //}
    }
}
