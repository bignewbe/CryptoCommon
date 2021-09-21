using CommonCSharpLibary.Facility;
using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using PortableCSharpLib;
using PortableCSharpLib.DataType;
using PortableCSharpLib.TechnicalAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortableCSharpLib.Interface;
using System.Threading;
//using static CommonCSharpLibary.EventHandlers;
//using static CryptoCommon.EventHandlers;

namespace CryptoCommon.Services
{
    public interface IDownLoadService
    {
        ServiceResult<QuoteBasicBase> Download(string symbol, int interval, int limit = 300);
    }

    public class QuoteService : ICrpytoQuoteService
    {
        ConcurrentDictionary<string, long> _lastSaveTime = new ConcurrentDictionary<string, long>();
        ConcurrentQueue<string> _quoteIdToSave = new ConcurrentQueue<string>();
        ConcurrentQueue<List<Ticker>> _tickerQueue = new ConcurrentQueue<List<Ticker>>();
        //ConcurrentQueue<List<OHLC>> _ohlcQueue = new ConcurrentQueue<List<OHLC>>();
        ConcurrentQueue<string> _symbolQueue = new ConcurrentQueue<string>();
        ConcurrentDictionary<string, OHLC> _candles = new ConcurrentDictionary<string, OHLC>();

        private System.Timers.Timer _timerSaveQuote = new System.Timers.Timer(2000);

        //HashSet<string> _symbols;
        protected IQuoteBasicFileStore _fileStore;
        protected ITickerStore _tickStore;
        protected IQuoteCaptureMemStore _qcStore;
        protected IQuoteBasicMemStore _qbStore;
        protected IDownLoadService _download;

        protected HashSet<string> _symbolsInitialized = new HashSet<string>();
        protected HashSet<string> _quoteIdInitialized = new HashSet<string>();

        private int _saveInterval = 300;
        private Random _random = new Random();
        private bool _isSaveQuoteBasicOngoing = false;
        private bool _isProcessTickerListOngoing = false;
        private bool _isProcessCandleListOngoing = false;

        //private bool _isCandleSubscribed = false;
        //private bool _isTickerSubscribed = false;

        public List<string> AvailableSymbols { get { return new List<string>(_symbolsInitialized); } }
        public string Exchange { get { return _qbStore.Exchange; } }
        //public virtual bool IsStarted { get { return _consumer.IsConnected; } }

        public event PortableCSharpLib.EventHandlers.QuoteBasicDataAddedOrUpdatedEventHandler OnQuoteBasicDataAddedOrUpated;
        public event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

        public QuoteService(IDownLoadService captureService, ITickerStore tickstore, IQuoteCaptureMemStore qcstore, IQuoteBasicMemStore qbstore, IQuoteBasicFileStore filestore)
        {
            //_symbols = new HashSet<string>(symbols);
            _tickStore = tickstore;
            _qcStore = qcstore;
            _qbStore = qbstore;
            _fileStore = filestore;
            _download = captureService;

            _fileStore.OnQuoteSaved += (object sender, string exchange, string filename)
                => Console.WriteLine($"quote saved to {filename}");
            _tickStore.OnTickerUpdated += _tickStore_OnTickerUpdated;
            _qcStore.OnQuoteCaptureDataAdded += _qcStore_OnQuoteCaptureDataAdded;
            _qbStore.OnQuoteBasicDataAddedOrUpdated += _qbStore_OnQuoteBasicDataAddedOrUpdated;

            _timerSaveQuote.Elapsed += _timer_Elapsed;
            _timerSaveQuote.Start();
            //_consumer = consumer;
            //_consumer.OnReceiveBroadcast += _consumer_OnReceiveBroadcast;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock(this)
            {
                if (_isSaveQuoteBasicOngoing) return;
                _isSaveQuoteBasicOngoing = !_isSaveQuoteBasicOngoing;
            }

            try
            {
                string quoteId;
                if (_quoteIdToSave.TryDequeue(out quoteId))
                {
                    _fileStore.Save(_qbStore.Quotes[quoteId]);
                }
            }
            finally
            {
                _isSaveQuoteBasicOngoing = !_isSaveQuoteBasicOngoing;
            }
        }

        //private void _consumer_OnReceiveBroadcast(object sender, string id, object data)
        //{
        //    Console.WriteLine(id);

        //    if (id == $"{this.Exchange}_OnCandleListRecevied")
        //    {
        //        var candles = data as List<OHLC>;
        //        _ohlcQueue.Enqueue(candles);
        //        Task.Run(() => this.ProcessCandleListQueue());
        //    }
        //    else if (id == $"{this.Exchange}_OnTickerListReceived")
        //    {
        //        var tickers = data as List<Ticker>;
        //        if (!_isCandleSubscribed)                                                  //use tickerlist only when no candlelist is subscribed
        //        {
        //            _tickerQueue.Enqueue(data as List<Ticker>);
        //            Task.Run(() => this.ProcessTickerListQueue());
        //        }
        //        //if (_symbolsInitialized.Contains(tickers[0].Symbol))
        //        //    OnTickerListReceived?.Invoke(this, this.Exchange, tickers);
        //    }
        //}

        public void AddCandleList(params OHLC[] candles)
        {
            lock (this)
            {
                //if (!_symbols.Contains(candles[0].Symbol)) return;
                var c = candles[0];
                if (!_candles.ContainsKey(c.Symbol)) 
                    _candles.TryAdd(c.Symbol, new OHLC());

                if (!_candles[c.Symbol].Equals(c))
                    _candles[c.Symbol].Copy(c);

                if (!_symbolQueue.Contains(c.Symbol))
                    _symbolQueue.Enqueue(c.Symbol);

                if (_symbolQueue.Count > 0)
                    this.ProcessCandleListQueue();
            }
        }

        public void AddTickerList(params Ticker[] tickers)
        {
            lock (this)
            {
                _tickStore.AddTickers(Exchange, tickers.ToList());
                //_tickerQueue.Enqueue(tickers);
                //Task.Run(() => this.ProcessTickerListQueue());
            }
        }

        protected void client_OnExceptionOccured(object sender, string exchange, Exception ex)
        {            
            OnExceptionOccured?.Invoke(sender, exchange, ex);
        }

        private void _tickStore_OnTickerUpdated(object sender, string exchange, Ticker ticker, double volume)
        {
            _qcStore.Add(ticker.Symbol, ticker.Timestamp, (ticker.Bid + ticker.Ask) / 2, volume);
        }

        private async void _qcStore_OnQuoteCaptureDataAdded(object sender, string exchange, IQuoteCapture quote, int numAppended)
        {
            try
            {
                var symbol = quote.Symbol;
                if (_symbolsInitialized.Contains(symbol))
                {
                    _qbStore.AddQuoteCapture(quote);
                }
                else   //initialize IQuoteBasicBase
                {
                    this.InitQuoteBasic(symbol);
                }
            }
            catch (Exception ex)
            {
                OnExceptionOccured?.Invoke(this, this.Exchange, ex);
            }
        }

        private void _qbStore_OnQuoteBasicDataAddedOrUpdated(object sender, string exchange, IQuoteBasicBase quote, int numAppended)
        {
            OnQuoteBasicDataAddedOrUpated?.Invoke(sender, this.Exchange, quote, numAppended);

            //var j = quote.Count - numAppended;
            //var lst = new List<OHLC>();
            //for (int i = j; i < quote.Count; i++)
            //{
            //    var ohlc = new OHLC
            //    {
            //        Symbol = quote.Symbol,
            //        Interval = quote.Interval,
            //        Time = quote.Time[i],
            //        Open = quote.Open[i],
            //        Close = quote.Close[i],
            //        High = quote.High[i],
            //        Low = quote.Low[i],
            //        Volume = quote.Volume[i]
            //    };
            //    lst.Add(ohlc);
            //}
            //OnCandleListReceived?.Invoke(this, this.Exchange, lst);

            // save quote
            if (!_quoteIdToSave.Contains(quote.QuoteID))
                _quoteIdToSave.Enqueue(quote.QuoteID);

            //var utcnow = DateTime.UtcNow.GetUnixTimeFromUTC();
            //if (!_lastSaveTime.ContainsKey(quote.QuoteID))
            //{
            //    var t = quote.Interval <= 900 ? utcnow + _random.Next(900) : 0;
            //    _lastSaveTime.TryAdd(quote.QuoteID, t);
            //}

            //if (utcnow - _lastSaveTime[quote.QuoteID] > _saveInterval)
            //{
            //    _quoteIdToSave.Enqueue(quote.QuoteID);
            //    _lastSaveTime[quote.QuoteID] = utcnow;
            //}

            //if (_quoteIdToSave.Count > 0)
            //    this.ProcessSaveQuoteBasicQueue();
        }

        private void InitQuoteBasic2(string symbol)
        {
            Console.WriteLine($"initializing {symbol}...");
            if (!_symbolsInitialized.Contains(symbol))
            {
                try
                {
                    var minInterval = 60;
                    var interval = minInterval;
                    var q = _fileStore.Load(symbol, interval, null, 2000);
                    if (q != null)
                        _qbStore.AddQuoteBasic(q, false);

                    var missingNum = q == null ? 2000 : (int)CFacility.Clip((DateTime.UtcNow.GetUnixTimeFromUTC() - q.LastTime) / interval + 10, 0, 2000);
                    if (missingNum > 0)
                    {
                        var retry = 0;
                        while (retry++ < 2)
                        {
                            var r = _download.Download(symbol, interval, missingNum);
                            if (r.Result)
                            {
                                var qb = r.Data;
                                _qbStore.AddQuoteBasic(qb, false);
                                _fileStore.Save(qb);
                                break;
                            }
                            Console.WriteLine($"\ntried count: {retry}, waiting to retry download {symbol} {interval}");
                            Thread.Sleep(1000);
                        }
                    }

                    //////////////////////////////////////////////////////////////////////////
                    /// initialize other intervals without making request to server
                    q = _qbStore.GetQuoteBasic(symbol, interval);
                    if (q != null && q.Count > 0)
                    {
                        _quoteIdInitialized.Add($"{symbol}_{interval}");

                        var q60 = new QuoteBasicBase(symbol, interval);
                        var q2 = _fileStore.Load(symbol, interval, null, 3000);
                        if (q2 != null)
                            q60.Append(q2);
                        q60.Append(q);

                        foreach (var intv in _qbStore.Intervals)
                        {
                            if (intv <= interval || intv % interval != 0)
                                continue;

                            var qb = new QuoteBasicBase(symbol, intv);
                            var q1 = _fileStore.Load(symbol, intv, null, 500);
                            if (q1 != null)
                                qb.Append(q1);
                            qb.Append(q60);

                            _qbStore.AddQuoteBasic(qb, false);
                            _fileStore.Save(qb);
                            _quoteIdInitialized.Add($"{symbol}_{intv}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnExceptionOccured?.Invoke(this, this.Exchange, ex);
                }

                _symbolsInitialized.Add(symbol);
                Console.WriteLine($"{symbol} initialized");
            }
        }

        private void InitQuoteBasic(string symbol)
        {
            Console.WriteLine($"initializing {symbol}...");
            if (!_symbolsInitialized.Contains(symbol))
            {
                var minInterval = 60;
                foreach (var interval in _qbStore.Intervals)
                {
                    if (interval >= minInterval)
                    {
                        try
                        {
                            var q = _fileStore.Load(symbol, interval, null, 2000);
                            if (q != null)
                                _qbStore.AddQuoteBasic(q, false);

                            var missingNum = q == null ? 2000 : (int)CFacility.Clip((DateTime.UtcNow.GetUnixTimeFromUTC() - q.LastTime) / interval + 10, 0, 2000);
                            if (missingNum > 0)
                            {
                                var retry = 0;
                                while (retry++ < 2)
                                {
                                    var r = _download.Download(symbol, interval, missingNum);
                                    if (r.Result)
                                    {
                                        var qb = r.Data;
                                        _qbStore.AddQuoteBasic(qb, false);
                                        _fileStore.Save(qb);
                                        break;
                                    }
                                    Console.WriteLine($"\ntried count: {retry}, waiting to retry download {symbol} {interval}");
                                    Thread.Sleep(1000);
                                }
                            }

                            q = _qbStore.GetQuoteBasic(symbol, interval);
                            if (q != null && q.Count > 0)
                                _quoteIdInitialized.Add($"{symbol}_{interval}");
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured?.Invoke(this, this.Exchange, ex);
                        }
                    }
                }

                _symbolsInitialized.Add(symbol);
                Console.WriteLine($"{symbol} initialized");
            }
        }

        public void ProcessCandleListQueue()
        {
            lock (this)
            {
                if (_isProcessCandleListOngoing) return;
                _isProcessCandleListOngoing = true;
            }

            Task.Run(() =>
            {
                try
                {
                    string symbol;
                    while (_symbolQueue.TryDequeue(out symbol))
                    {
                        var d = _candles[symbol];
                        if (_symbolsInitialized.Contains(symbol))
                            _qbStore.AddCandle(d.Symbol, d.Interval, d.Time, d.Open, d.Close, d.High, d.Low, d.Volume, true);
                        else
                            this.InitQuoteBasic2(symbol);
                    }
                }
                catch (Exception ex)
                {
                    OnExceptionOccured?.Invoke(this, Exchange, ex);
                }
                finally
                {
                    _isProcessCandleListOngoing = false;
                }
            });
        }

        //private void ProcessTickerListQueue()
        //{
        //    lock (this)
        //    {
        //        if (_isProcessTickerListOngoing) return;
        //        _isProcessTickerListOngoing = true;
        //    }

        //    try
        //    {
        //        List<Ticker> ticker;
        //        while (_tickerQueue.TryDequeue(out ticker))
        //        {
        //            foreach (var t in ticker)
        //                _tickStore.AddTickers(this.Exchange, t);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        OnExceptionOccured?.Invoke(this, this.Exchange, ex);
        //    }
        //    finally
        //    {
        //        _isProcessTickerListOngoing = false;
        //    }
        //}

        //private void ProcessSaveQuoteBasicQueue()
        //{
        //    lock (this)
        //    {
        //        if (_isSaveQuoteBasicOngoing) return;
        //        _isSaveQuoteBasicOngoing = true;
        //    }

        //    Task.Run(() =>
        //    {
        //        try
        //        {
        //            string quoteId;
        //            while (_quoteIdToSave.TryDequeue(out quoteId))
        //            {
        //                //Console.WriteLine($"saving {quoteId}...");
        //                _fileStore.Save(_qbStore.Quotes[quoteId]);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.ToString());
        //            OnExceptionOccured?.Invoke(this, this.Exchange, ex);
        //        }
        //        finally
        //        {
        //            _isSaveQuoteBasicOngoing = false;
        //        }
        //    });
        //}

        public ServiceResult<QuoteCapture> GetInMemoryQuoteCapture(string symbol)
        {
            return ServiceResult<QuoteCapture>.CallAsyncFunction(() => Task.Run(() => (QuoteCapture) _qcStore.GetInMemoryQuoteCapture(symbol))).Result;
        }

        public ServiceResult<QuoteBasicBase> GetInMemoryQuoteBasic(string symbol, int interval)
        {
            var r = ServiceResult<QuoteBasicBase>.CallAsyncFunction(() => Task.Run(() => _qbStore.GetQuoteBasic(symbol, interval))).Result;
            return r;
        }

        public ServiceResult<List<string>> GetAvaliableSymbols()
        {
            return new ServiceResult<List<string>> { Result = true, Data = new List<string>(_symbolsInitialized) };
        }

        public ServiceResult<List<string>> GetAvaliableQuoteIds()
        {
            return new ServiceResult<List<string>> { Result = true, Data = new List<string>(_quoteIdInitialized) };
        }

        public ServiceResult<string> GetExchange()
        {
            return new ServiceResult<string> { Result = true, Data = this.Exchange };
        }

        public ServiceResult<QuoteBasicBase> GetQuoteBasic(string symbol, int interval, long stime, int num)
        {
            try
            {
                var q = new QuoteBasicBase(symbol, interval);
                var q1 = _fileStore.Load(symbol, interval, stime, num);
                var q2 = _qbStore.GetQuoteBasic(symbol, interval);
                if (q1 != null) q.Append(q1, false);
                if (q2 != null) q.Append(q2, false);
                return new ServiceResult<QuoteBasicBase> { Result = true, Data = q };
            }
            catch(Exception ex)
            {
                return new ServiceResult<QuoteBasicBase> { Result = false };
            }
        }
    }
}
