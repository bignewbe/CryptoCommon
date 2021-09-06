using CryptoCommon.DataTypes;
using CryptoCommon.Services;
using Newtonsoft.Json;
using PortableCSharpLib;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public class OrderProxyBase
    {
        private ITradeService _trade;

        private bool _isNeedSaveOrders = false;
        private bool _isLoadAndDumpFileBusy = false;
        private bool _isProcessOrdersTosubmitBusy = false;
        private bool _isProcessOrdersToCheckQueueBusy = false;
        private bool _isProcessBalanceBusy = false;
        private bool _isProcessPullOrderBusy = false;

        private long _currentTime;
        protected ConcurrentDictionary<string, SyncStatus> _syncOrderAction = new ConcurrentDictionary<string, SyncStatus>();

        private System.Timers.Timer _timer1 = new System.Timers.Timer();
        private bool _IsStarted = false;
        public bool IsStarted
        {
            get { return _IsStarted; }
            set
            {
                if (_IsStarted != value)
                {
                    _IsStarted = value; OnStateChanged?.Invoke(this, value);
                }
            }
        }

        public List<string> Symbols { get; private set; }
        public ConcurrentDictionary<string, FZOrder> OpenOrders { get; private set; } = new ConcurrentDictionary<string, FZOrder>();
        public ConcurrentDictionary<string, FZOrder> ClosedOrders { get; private set; } = new ConcurrentDictionary<string, FZOrder>();

        //private ConcurrentQueue<string> _orderIdToUpdateTimeLast = new ConcurrentQueue<string>();
        private ConcurrentQueue<FZOrder> _orderToCheck = new ConcurrentQueue<FZOrder>();
        private ConcurrentQueue<string> _queuePullOrder = new ConcurrentQueue<string>();

        //private IRateGate _gate = new RateGate(20, new TimeSpan(0, 0, 2));;         //in simulation mode we remove gate check
        protected bool _isSimulationMode;  //in simulation mode we does not start timer loop to check order status, alternatviely check order status is triggered externally
        private double _timerinterval;

        //long _lastUpdateOrderTime;
        private ConcurrentDictionary<string, long> _lastUpdateOrderTimePerSymbol = new ConcurrentDictionary<string, long>();
        private int _pollOrderIntervalPerSymbol = 60;
        private string _fileNameDump;
        protected bool _isEnableLog;

        //private ConcurrentDictionary<string, long> _lastUpdateTime = new ConcurrentDictionary<string, long>();
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;
        public event CryptoCommon.EventHandlers.StateChangedEventHandler OnStateChanged;
        //public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderCancelled;
        //public event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

        public OrderProxyBase(ITradeService trade, List<string> symbols, string dumpfile, int timerInterval, bool isSimuationMode, bool isEnableLog)
        {
            _trade = trade;
            _isSimulationMode = isSimuationMode;
            _timerinterval = timerInterval;
            _isEnableLog = isEnableLog;

            this.Symbols = symbols;
            var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
            for (int i = 0; i < symbols.Count; i++)
                _lastUpdateOrderTimePerSymbol.TryAdd(symbols[i], tnow + 120 + i * _pollOrderIntervalPerSymbol);
            _fileNameDump = dumpfile;
        }

        private void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
            foreach (var symbol in Symbols)
            {
                if (tnow - _lastUpdateOrderTimePerSymbol[symbol] >= _pollOrderIntervalPerSymbol)
                {
                    _lastUpdateOrderTimePerSymbol[symbol] = tnow;
                    if (!_queuePullOrder.Contains(symbol))
                    {
                        _queuePullOrder.Enqueue(symbol);
                        this.ProcessUpdateOrderQueue();
                    }
                }
            }

            if (_isNeedSaveOrders)
            {
                lock (this)
                {
                    if (_isLoadAndDumpFileBusy) return;
                    _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
                }

                try
                {
                    this.DumpToFile();
                }
                catch (Exception ex)
                {
                    this.LogDebug("Ignore dumping error!");
                    this.LogDebug(ex.ToString());
                }
                finally
                {
                    this.SetDumpFile(false);
                    _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
                }
            }
        }


        #region update orders
        public void UpdateOrders(params FZOrder[] orders)
        {
            if (!this.IsStarted) return;

            if (_isSimulationMode)
            {
                foreach (var o in orders) this.CheckOrderLocal(o, true);
            }
            else
            {
                foreach (var o in orders)
                {
                    this.LogDebug($"new order received from server: {ConvertOrderToStr(o)}");
                    this.RemoveRefId(o);
                    _orderToCheck.Enqueue(o);
                }
                this.ProcessOrdersToCheckQueue();
            }
        }

        /// <summary>
        /// 1. get open orders from remote 
        /// 2. if exist in local memory => update => done by checkOrder
        /// 3. if not exist in local memory => add => done by checkOrder
        /// 4. if local order not exist in remote orders => remove from open order list => done by chckOrder in update closed orders
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isTriggerEvent"></param>
        private void UpdteOpenOrdersFromServer(string symbol, bool isTriggerEvent)
        {
            var r1 = _trade.GetOpenOrdersBySymbol(symbol);
            if (r1.Result && r1.Data != null)
            {
                foreach (var o in r1.Data)
                    this.CheckOrderLocal(o, isTriggerEvent);

                var d = r1.Data.ToDictionary(o => o.OrderId, o => o);
                var openOrders = this.OpenOrders.Values.Where(o => o.Symbol == symbol);
                foreach (var o in openOrders)
                {
                    if (!d.ContainsKey(o.OrderId))
                        this.UpdateLastTimeAndCheckOrderLocal(o, isTriggerEvent);
                }
            }
        }

        /// <summary>
        /// 1. exist in local open orders
        /// 2. not exist in local closed orders
        /// 3. exist in local closed orders by TimeLast not correct
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isTriggerEvent"></param>
        private void UpdateCloseOrdersFromServer(string symbol, bool isTriggerEvent)
        {
            //this.LogDebug($"update closed orders for {symbol}");
            var count = 0;
            var r2 = _trade.GetClosedOrdersBySymbol(symbol, false);
            if (r2.Result && r2.Data != null)
            {
                foreach (var o in r2.Data)
                {
                    if ((this.OpenOrders.ContainsKey(o.OrderId)) ||
                        (!this.ClosedOrders.ContainsKey(o.OrderId)) ||
                        (this.ClosedOrders.ContainsKey(o.OrderId) && this.ClosedOrders[o.OrderId].TimeLast <= this.ClosedOrders[o.OrderId].TimeCreated))
                    {
                        //if (!this.ClosedOrders.ContainsKey(o.OrderId))
                        //    this.ClosedOrders.TryAdd(o.OrderId, o);
                        var r = _trade.GetLastTimeForOrder(o.OrderId, o.Symbol);
                        if (!_isSimulationMode) Thread.Sleep(50);
                        if (r.Result && o.TimeLast <= (r.Data + 10).GetUTCFromUnixTime())
                        {
                            o.TimeLast = r.Data.GetUTCFromUnixTime();
                            if (this.ClosedOrders.ContainsKey(o.OrderId))
                            {
                                //this.LogDebug("update last time directly");
                                //this.LogDebug($"TimeLast updated for closed order: {ConvertOrderToStr(o)}");
                                this.ClosedOrders[o.OrderId].TimeLast = r.Data.GetUTCFromUnixTime();
                                this.SetDumpFile(true);
                            }
                            this.CheckOrderLocal(o, isTriggerEvent);
                        }

                        ++count;
                        if (count > 20)
                        {
                            count = 0;
                            if (!_isSimulationMode) Thread.Sleep(2000);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// for closed order, TimeLast is not correct. trade.CheckOrder will update the TimeLast from server
        /// </summary>
        /// <param name="order"></param>
        /// <param name="isTriggerEvent"></param>
        private void UpdateLastTimeAndCheckOrderLocal(FZOrder order, bool isTriggerEvent)
        {
            if (order != null)
            {
                var isStopOrder = order.Ordertype == OrderType.stop_buy || order.Ordertype == OrderType.stop_sell;
                var r = _trade.CheckOrder(order.Symbol, order.OrderId, isStopOrder);
                if (!_isSimulationMode) Thread.Sleep(50);
                if (r.Result && r.Data != null)
                    this.CheckOrderLocal(r.Data, isTriggerEvent);
            }
        }

        public void UpdateTime(long time)
        {
            _currentTime = time;
        }

        public DateTime GetCurrentTime()
        {
            return _isSimulationMode ? _currentTime.GetUTCFromUnixTime() : DateTime.UtcNow;
        }

        protected void AddRefId(FZOrder order)
        {
            if (!_syncOrderAction.ContainsKey(order.Symbol))
            {
                Func<long> fun = () => this.GetCurrentTime().GetUnixTimeFromUTC();
                var s = new SyncStatus(300, fun);
                _syncOrderAction.TryAdd(order.Symbol, s);
            }
            _syncOrderAction[order.Symbol].AddRefId(order.RefId);
        }

        private void RemoveRefId(FZOrder order)
        {
            if (_syncOrderAction.ContainsKey(order.Symbol))
                _syncOrderAction[order.Symbol].RemoveRefId(order.RefId);
        }

        private bool CheckOrderLocal(FZOrder o, bool isTriggerEvent)
        {
            if (o == null) return false;
            lock (this)
            {
                this.RemoveRefId(o);

                var isUpdated = false;
                var isExistingOpen = OpenOrders.ContainsKey(o.OrderId);
                var isExistingClose = ClosedOrders.ContainsKey(o.OrderId);
                var isNewOrder = !isExistingOpen && !isExistingClose;
                var isOrderChanged = isExistingOpen ? !OpenOrders[o.OrderId].Equals(o) : (isExistingClose ? !ClosedOrders[o.OrderId].Equals(o) : false);
                var isOrderOpen = o.IsOrderOpen();

                //existOpen  existClose  isOpen  isChanged
                //Y          N           Y       Y          => existing open order updated
                //Y          N           N       -          => existing open order closed
                //N          Y           Y       -          => invalid
                //N          Y           N       Y          => existing order updated
                //N          N           Y       -          => new order opened
                //N          N           N       -          => missed order closed
                if (isExistingOpen && !isExistingClose && isOrderOpen && isOrderChanged)
                {
                    this.LogDebug($"existing open order updated: {ConvertOrderToStr(o)}");
                    this.OpenOrders[o.OrderId].Copy(o);
                    if (isTriggerEvent) OnOrderUpdated(this, o);
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (isExistingOpen && !isExistingClose && !isOrderOpen)
                {
                    this.OpenOrders[o.OrderId].Copy(o);
                    if (isTriggerEvent) OnOrderClosed(this, o);

                    FZOrder order;
                    if (OpenOrders.TryRemove(o.OrderId, out order))
                    {
                        //if (o.OrderId == "orderId_10")
                        //    Console.WriteLine();

                        this.LogDebug($"order removed from open order list: {ConvertOrderToStr(o)}");
                        //if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                        if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, null);
                    }
                    if (ClosedOrders.TryAdd(o.OrderId, o))
                    {
                        this.LogDebug($"order added to closed order list: {ConvertOrderToStr(o)}");
                        //if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                        if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, null);
                    }
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (!isExistingOpen && isExistingClose && isOrderOpen)
                {
                    this.LogDebug($"order exist in close list while it is still open: {ConvertOrderToStr(o)}");
                    FZOrder order;
                    if (ClosedOrders.TryRemove(o.OrderId, out order))
                        //if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                        if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, null);

                    if (OpenOrders.TryAdd(o.OrderId, o))
                    {
                        if (isTriggerEvent)
                        {
                            this.OnOrderOpened?.Invoke(this, o);
                            //this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                            this.OnOpenOrderListChanged?.Invoke(this, null);
                        }
                    }
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (!isExistingOpen && isExistingClose && !isOrderOpen && isOrderChanged)
                {
                    this.LogDebug($"existing closed order updated: {ConvertOrderToStr(o)}");
                    this.ClosedOrders[o.OrderId].Copy(o);
                    if (isTriggerEvent) OnOrderUpdated(this, o);
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (!isExistingOpen && !isExistingClose && isOrderOpen)
                {
                    //this.LogDebug($"new order opened: {ConvertOrderToStr(o)}");
                    if (isTriggerEvent) this.OnOrderOpened?.Invoke(this, o);
                    if (this.OpenOrders.TryAdd(o.OrderId, o))
                    {
                        this.LogDebug($"new order added to open order list: {ConvertOrderToStr(o)}");
                        //if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                        if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, null);
                    }
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (!isExistingOpen && !isExistingClose && !isOrderOpen)
                {
                    //this.LogDebug($"missed order closed: {ConvertOrderToStr(o)}");
                    if (isTriggerEvent) OnOrderClosed(this, o);

                    if (ClosedOrders.TryAdd(o.OrderId, o))
                    {
                        this.LogDebug($"missed order added to closed order list: {ConvertOrderToStr(o)}");
                        //if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                        if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, null);
                    }
                    isUpdated = true;
                    this.SetDumpFile(true);
                }

                //if (!isUpdated && !o.IsOrderOpen())
                //{
                //    this.LogDebug($"order not updated: {o.Symbol} {o.Ordertype} isInOpenList = {isExistingOpen}, isInCloseList = {isExistingClose}, isOrderChanged = {isOrderChanged}, isOrderOpen = {isOrderOpen}");
                //}

                return isUpdated;
            }
        }
        #endregion

        #region processing queue
        /// <summary>
        /// check individual order from server
        /// </summary>
        void ProcessOrdersToCheckQueue()
        {
            lock (this)
            {
                if (_isProcessOrdersToCheckQueueBusy) return;
                _isProcessOrdersToCheckQueueBusy = true;
            }

            Task.Run(() =>
            {
                try
                {
                    FZOrder o;
                    var count = 0;
                    while (_orderToCheck.TryDequeue(out o))
                    {
                        if (o.IsOrderOpen())
                            this.CheckOrderLocal(o, true);
                        else
                        {
                            this.UpdateLastTimeAndCheckOrderLocal(o, true);
                            ++count;
                            if (count > 10)
                            {
                                if (!_isSimulationMode) Thread.Sleep(2000);
                                count = 0;
                            }
                        }
                    }
                }
                finally
                {
                    _isProcessOrdersToCheckQueueBusy = false;
                }
            });
        }

        //1. pull openOrders and closeOrders from time to time from server, such that server and local cache are always in sync with each other
        void ProcessUpdateOrderQueue()
        {
            lock (this)
            {
                if (_isProcessPullOrderBusy) return;
                _isProcessPullOrderBusy = !_isProcessPullOrderBusy;
            }

            Task.Run(() =>
            {
                try
                {
                    string symbol;
                    while (_queuePullOrder.TryDequeue(out symbol))
                    {
                        //////////////////////////////////////////////////////////////////////////////
                        ///update open orders
                        this.UpdteOpenOrdersFromServer(symbol, true);

                        /////////////////////////////////////////////////////////////////////////
                        ///update closed orders
                        this.UpdateCloseOrdersFromServer(symbol, true);
                    }
                }
                finally
                {
                    _isProcessPullOrderBusy = !_isProcessPullOrderBusy;
                }
            });
        }
        #endregion

        #region start/stop/initialize/pull data
        public void Start()
        {
            if (!_isSimulationMode)
            {
                if (!IsStarted)
                {
                    this.Initialize();
                    _timer1.Interval = _timerinterval;
                    _timer1.Elapsed += _timer1_Elapsed;
                    _timer1.Start();
                    IsStarted = true;
                }
            }
            else
            {
                IsStarted = true;
            }
        }

        public void Stop()
        {
            if (!_isSimulationMode)
            {
                if (IsStarted)
                {
                    _timer1.Elapsed -= _timer1_Elapsed;
                    _timer1.Stop();
                    IsStarted = false;
                }
            }
            else IsStarted = false;
        }

        void Initialize()
        {
            lock (this)
            {
                if (_isProcessOrdersToCheckQueueBusy || _isProcessOrdersTosubmitBusy || _isProcessBalanceBusy) return;
                _isProcessBalanceBusy = _isProcessOrdersToCheckQueueBusy = _isProcessOrdersTosubmitBusy = true;
            }

            try
            {
                this.LoadFromFile();

                ///initialize orders
                this.OpenOrders.Clear();
                foreach (var s in Symbols)
                {
                    Console.WriteLine($"update orders for {s}...");
                    this.UpdteOpenOrdersFromServer(s, false);
                    this.UpdateCloseOrdersFromServer(s, false);
                }

                var tnow = DateTime.UtcNow;
                foreach (var o in this.ClosedOrders.Values)
                {
                    if (o.State == OrderState.fully_filled && o.TimeCreated >= o.TimeLast && (tnow - o.TimeCreated).TotalSeconds < 86400 * 30)
                    {
                        Console.WriteLine($"updating TimeLast for {ConvertOrderToStr(o)}");
                        var r = _trade.GetLastTimeForOrder(o.OrderId, o.Symbol);
                        if (!_isSimulationMode) Thread.Sleep(105);
                        if (r.Result && o.TimeLast < r.Data.GetUTCFromUnixTime())
                        {
                            Console.WriteLine("TimeLast updated!");
                            o.TimeLast = r.Data.GetUTCFromUnixTime();
                            this.SetDumpFile(true);
                        }
                    }
                }

                if (this.OpenOrders.Count > 0)
                    this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(o => new FZOrder(o)));

                if (this.ClosedOrders.Count > 0)
                    this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(o => new FZOrder(o)));
            }
            finally
            {
                _isProcessBalanceBusy = _isProcessOrdersToCheckQueueBusy = _isProcessOrdersTosubmitBusy = false;
            }
        }

        void DumpToFile()
        {
            lock (this)
            {
                var delimiter = "\nqxdjt394";
                var str = new StringBuilder();
                var datestr = DateTime.UtcNow.ToString("yyyyMMdd");
                var datapath = Path.GetDirectoryName(_fileNameDump);

                str.Append(JsonConvert.SerializeObject(OpenOrders, Formatting.Indented) + delimiter);
                str.Append(JsonConvert.SerializeObject(ClosedOrders, Formatting.Indented) + delimiter);

                File.WriteAllText(_fileNameDump, str.ToString());
                File.WriteAllText(Path.Combine(datapath, $"{datestr}_{Path.GetFileName(_fileNameDump)}"), str.ToString());
            }
        }

        void LoadFromFile()
        {
            lock (this)
            {
                try
                {
                    if (File.Exists(this._fileNameDump))
                    {
                        var delimiter = "\nqxdjt394";
                        var str = File.ReadAllText(this._fileNameDump);
                        var items = Regex.Split(str, delimiter);

                        this.OpenOrders = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FZOrder>>(items[0]);
                        this.ClosedOrders = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FZOrder>>(items[1]);

                        var orders = this.ClosedOrders.Values.Where(o => o.DealAmount > 0 || o.State != OrderState.cancelled).ToList();
                        this.ClosedOrders = new ConcurrentDictionary<string, FZOrder>(orders.ToDictionary(o => o.OrderId, o => o));
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                    this.LogDebug(ex.ToString());
                }
            }
        }
        #endregion

        #region util
        protected void LogDebug(string str)
        {
            if (_isEnableLog)
                Log.Debug("Proxy: " + str);
        }

        protected string ConvertOrderToStr(FZOrder o)
        {
            return $"{o.RefId} {o.OrderId} {o.Ordertype} {o.Symbol} price={o.Price} amount={o.Amount} {o.State} {o.TimeCreated} {o.TimeLast}";
        }

        void SetDumpFile(bool v)
        {
            lock (this)
            {
                _isNeedSaveOrders = v;
            }
        }
        #endregion
    }
}
