using CryptoCommon.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Models
{
    public class QuoteCapturerWithNetwork : IQuoteCapturer
    {
        IQuoteCapturer _capturer;
        ITcpServerGeneral _tcpServer;

        public event CryptoCommon.EventHandlers.TickerReceivedEventHandler OnTickerReceived;
        public event CryptoCommon.EventHandlers.TickerReceivedEventHandlerList OnTickerListReceived;
        public event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;
        public event PortableCSharpLib.TechnicalAnalysis.EventHandlers.DataAddedOrUpdatedEventHandler OnQuoteBasicDataAddedOrUpated;
        public event CryptoCommon.EventHandlers.QuoteSavedEventHandler OnQuoteSaved;

        public string Exchange { get { return _capturer.Exchange; } }
        public string DataFolder { get { return _capturer.Exchange; } }
        public int MaxNumTicker { get { return _capturer.MaxNumTicker; } }
        public int MaxNumBars { get { return _capturer.MaxNumBars; } }
        public bool IsStarted { get { return _capturer.IsStarted && _tcpServer.IsActive; } }

        public List<int> Intervals { get { return _capturer.Intervals; } }

        public QuoteCapturerWithNetwork(ITcpServerGeneral tcpServer, IQuoteCapturer capturer)
        {
            _tcpServer = tcpServer;
            _tcpServer.OnHandleRequestGeneralDataEvent += _tcpServer_OnHandleRequestGeneralDataEvent;

            _capturer = capturer;
            _capturer.OnTickerReceived += _capturer_OnTickerReceived;
            _capturer.OnTickerListReceived += _capturer_OnTickerListReceived;
            _capturer.OnExceptionOccured += _capturer_OnExceptionOccured;
            _capturer.OnQuoteBasicDataAddedOrUpated += _capturer_OnQuoteBasicDataAddedOrUpated;
            _capturer.OnQuoteSaved += _capturer_OnQuoteSaved;
        }

        private void _capturer_OnQuoteSaved(object sender, string filename)
        {
            OnQuoteSaved?.Invoke(sender, filename);
        }

        private void _capturer_OnQuoteBasicDataAddedOrUpated(object sender, IQuoteBasic quote, int numAppended)
        {
            OnQuoteBasicDataAddedOrUpated?.Invoke(sender, quote, numAppended);
        }

        private void _capturer_OnExceptionOccured(object sender, string exchange, Exception ex)
        {
            OnExceptionOccured?.Invoke(sender, exchange, ex);
        }

        private object _tcpServer_OnHandleRequestGeneralDataEvent(object sender, int clientId, string id, object parameter)
        {
            try
            {
                if (id == $"RP_RequestAvailableSymbols_{_capturer.Exchange}")
                    return _capturer.GetAvaliableSymbols();
                else if (id == $"RP_GetInMemoryQuoteCapture_{_capturer.Exchange}")
                {
                    var symbol = parameter as string;
                    return _capturer.GetInMemoryQuoteCapture(symbol);
                }
                else if (id == $"RP_GetInMemoryQuoteBasic_{_capturer.Exchange}")
                {
                    var items = (parameter as string).Split('.');
                    var symbol = items[0];
                    var interval = int.Parse(items[1]);
                    return _capturer.GetInMemoryQuoteBasic(symbol, interval);
                }
                return null;
            }
            catch (Exception e)
            {
                OnExceptionOccured?.Invoke(sender, _capturer.Exchange, e);
                return null;
            }
        }

        private void _capturer_OnTickerReceived(object sender, string exchange, CryptoCommon.DataTypes.Ticker ticker)
        {
            OnTickerReceived?.Invoke(sender, exchange, ticker);
            _tcpServer.Broadcast($"Ticker_{this.Exchange}", ticker);
        }

        //update ticker list
        private void _capturer_OnTickerListReceived(object sender, string exchange, List<Ticker> ticker)
        {
            OnTickerListReceived?.Invoke(sender, exchange, ticker);
            _tcpServer.Broadcast($"TickerList_{this.Exchange}", ticker);
        }

        public void Start()
        {
            _capturer.Start();
            _tcpServer.StartAsync();
            while (!_tcpServer.IsActive) Thread.Sleep(10);
        }

        public void Stop()
        {
            _capturer.Stop();
            _tcpServer.StopAsync();
            while (_tcpServer.IsActive) Thread.Sleep(10);
        }

        public IQuoteCapture GetInMemoryQuoteCapture(string symbol)
        {
            return _capturer.GetInMemoryQuoteCapture(symbol);
        }

        public List<string> GetAvaliableSymbols()
        {
            return _capturer.GetAvaliableSymbols();
        }

        public IQuoteBasic GetInMemoryQuoteBasic(string symbol, int interval)
        {
            return _capturer.GetInMemoryQuoteBasic(symbol, interval);
        }
    }
}
