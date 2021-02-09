using CryptoCommon;
using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib;
using PortableCSharpLib.DataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CryptoCommonUnitTest
{
    public class MockSocketTicker
    {
        //private Timer _timerTicker;
        private int _milisecondsTickerInterval = 5;
        private bool _IsStarted = false;
        private long _timenow = DateTime.UtcNow.GetUnixTimeFromUTC();
        private int _count = 0;

        public bool IsStarted
        {
            get { return _IsStarted; }
            set
            {
                if (_IsStarted != value)
                {
                    _IsStarted = value; OnCaptureStateChanged?.Invoke(this, Exchange, value);
                }
            }
        }

        public string Exchange { get; private set; }
        public HashSet<string> SubscribedStandardSymbols => throw new NotImplementedException();

        public event EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        public event EventHandlers.CaptureStateChangedEventHandler OnCaptureStateChanged;
        public event EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

        public MockSocketTicker(int timerInterval)
        {
            _milisecondsTickerInterval = timerInterval;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                var symbols = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
                for (int i = 0; i < 1000; i++)
                {
                    var tickers = symbols.Select(s => new Ticker
                    {
                        Symbol = s,
                        Timestamp = _timenow + i,
                    }).ToList();
                    OnTickerListReceived?.Invoke(this, Exchange, tickers);
                }
            });
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
               
        //public void Start()
        //{
        //    lock (this)
        //    {
        //        if (IsStarted) return;

        //        IsStarted = true;
        //        _timerTicker = new Timer();
        //        _timerTicker.Interval = _milisecondsTickerInterval;
        //        _timerTicker.Elapsed += _timerTicker_Elapsed;
        //        _timerTicker.Start();
        //    }
        //}

        //public void Stop()
        //{
        //    lock (this)
        //    {
        //        if (!IsStarted) return;

        //        _timerTicker.Stop();
        //        _timerTicker.Elapsed -= _timerTicker_Elapsed;
        //        IsStarted = false;
        //    }
        //}

        //private void _timerTicker_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    var symbols = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
        //    var tickers = symbols.Select(s => new Ticker
        //    {
        //        Symbol = s,
        //        Timestamp = _timenow + _count,
        //    }).ToList();

        //    ++_count;
        //    OnTickerListReceived?.Invoke(this, Exchange, tickers);
        //}
    }
}
