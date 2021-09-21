using CryptoCommon.DataTypes;
using CryptoCommon.Interfaces;
using Newtonsoft.Json;
using PortableCSharpLib;
using PortableCSharpLib.Model;
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

namespace CryptoCommon.Services
{
    public class ManualCommand<T> where T : class
    {
        public CommandId CommandId { get; set; }          //identify what command
        public T Order { get; set; }
    }

    public class SpotProxy : CINotification, ISpotProxy
    {
        static DateTime _compareTime = DateTime.Parse("2019/01/01");

        private ISpotTradeService _trade;
        private ConcurrentDictionary<string, SyncStatus> _syncOrderAction;

        //string _exchange;
        //string _symbol;
        //private ICryptoCaptureService _capture;  
        //private string _filenameClosedOrders;
        //private string _filenameOpenOrders;
        //private string _filenameOrderToSubmit;
        //private string _filenameOrderToCheck;
        //private string _filenameRefIdToPrevRefId;
        //private string _filenameOrderIdToTimeLast;
        //private string _filenameOrderIdToTimeCreated;

        private bool _isNeedSaveOrders = false;
        private bool _isLoadAndDumpFileBusy = false;
        private bool _isProcessOrdersTosubmitBusy = false;
        private bool _isProcessOrdersToCheckQueueBusy = false;
        private bool _isProcessBalanceBusy = false;
        private bool _isProcessPullOrderBusy = false;

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
        //private List<string> _orderIdsToCheck = new List<string>();
        //private ConcurrentDictionary<string, Ticker> _tickers = new ConcurrentDictionary<string, Ticker>();
        //private ConcurrentDictionary<string, int> _leverages = new ConcurrentDictionary<string, int>();
        //private Dictionary<string, DateTime> _orderIdToTimeCreated = new Dictionary<string, DateTime>();
        //private Dictionary<string, DateTime> _orderIdToTimeLast = new Dictionary<string, DateTime>();

        public ConcurrentDictionary<string, SpotBalance> Balances { get; private set; } = new ConcurrentDictionary<string, SpotBalance>();
        public ConcurrentDictionary<string, FZOrder> OpenOrders { get; private set; } = new ConcurrentDictionary<string, FZOrder>();
        public ConcurrentDictionary<string, FZOrder> ClosedOrders { get; private set; } = new ConcurrentDictionary<string, FZOrder>();

        //private Dictionary<string, string> _refIdToPrevRefId = new Dictionary<string, string>();
        private ConcurrentQueue<ManualCommand<FZOrder>> _ordersToSubmit = new ConcurrentQueue<ManualCommand<FZOrder>>();

        //private ConcurrentQueue<string> _orderIdToUpdateTimeLast = new ConcurrentQueue<string>();
        private ConcurrentQueue<FZOrder> _orderToCheck = new ConcurrentQueue<FZOrder>();
        private ConcurrentQueue<SpotBalance> _balanceToUpdate = new ConcurrentQueue<SpotBalance>();
        private ConcurrentQueue<string> _queuePullOrder = new ConcurrentQueue<string>();

        //private IRateGate _gate = new RateGate(20, new TimeSpan(0, 0, 2));;         //in simulation mode we remove gate check
        private bool _isSimulationMode;  //in simulation mode we does not start timer loop to check order status, alternatviely check order status is triggered externally
        private double _timerinterval;
        private long _lastUpdateBalanceTime;

        //long _lastUpdateOrderTime;
        private ConcurrentDictionary<string, long> _lastUpdateOrderTimePerSymbol = new ConcurrentDictionary<string, long>();
        private int _pollOrderIntervalPerSymbol = 60;
        private int _pollBalanceInterval = 60;
        private long _currentTime;
        private string _fileNameDump;
        private bool _isEnableLog;

        //private HashSet<string> _checkedOrderIds = new HashSet<string>();

        //private ConcurrentDictionary<string, long> _lastUpdateTime = new ConcurrentDictionary<string, long>();
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderCancelled;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotBalance> OnCurrencyBalanceUpdated;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotBalance>> OnAccountBalanceUpdated;
        public event CryptoCommon.EventHandlers.StateChangedEventHandler OnStateChanged;
        public event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

        public SpotProxy(ISpotTradeService trade, List<string> symbols, string dumpfile, int timerInterval, bool isSimuationMode, bool isEnableLog)
        {
            _trade = trade;
            _isSimulationMode = isSimuationMode;
            _timerinterval = timerInterval;
            _isEnableLog = isEnableLog;

            Func<long> fun = () => this.GetCurrentTime().GetUnixTimeFromUTC();
            _syncOrderAction = new ConcurrentDictionary<string, SyncStatus>(); //new SyncStatus(300, fun);

            this.Symbols = symbols;
            var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
            //while (symbols.Count * _pollOrderIntervalPerSymbol < 600)  ++_pollOrderIntervalPerSymbol;
            //while (symbols.Count * _pollOrderIntervalPerSymbol > 3600) --_pollOrderIntervalPerSymbol;
            //if (_pollOrderIntervalPerSymbol < 60) _pollOrderIntervalPerSymbol = 60;
            //var rnd = new Random();
            //var delta = _pollServerIntervalPerSymbol;
            for (int i = 0; i < symbols.Count; i++)
            {
                //var t = rnd.Next(0, _pollServerInterval);
                _lastUpdateOrderTimePerSymbol.TryAdd(symbols[i], tnow + 120 + i * _pollOrderIntervalPerSymbol);
            }
            _lastUpdateBalanceTime = tnow;
            _fileNameDump = dumpfile;

            //_fileNameDump = Path.Combine(datapath, "orderproxy-spot.dump");
            //_filenameOpenOrders = Path.Combine(datapath, "spotOpenOrders.json");
            //_filenameClosedOrders = Path.Combine(datapath, "spotClosedOrders.json");
            //_filenameOrderToSubmit = Path.Combine(datapath, "spotOrderToSubmit.json");
            //_filenameOrderToCheck = Path.Combine(datapath, "spotOrderToCheck.json");
            //_filenameRefIdToPrevRefId = Path.Combine(datapath, "spotRefIdToPrevId.json");
            //_filenameOrderIdToTimeLast = Path.Combine(datapath, "spotOrderIdToTimeLast.json");
            //_filenameOrderIdToTimeCreated = Path.Combine(datapath, "spotOrderIdToTimeCreated.json");
            //this.PropertyChanged += OrderManager_PropertyChanged;
            //this.OnOpenOrderListChanged += (object sender, IList<FZOrder> orders) => this.LogDebug($"order  added: {order.RefId} {OpenOrders[orderId].OrderId} {OpenOrders[orderId].Symbol}");
        }

        private void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
            if (tnow - _lastUpdateBalanceTime > _pollBalanceInterval)
            {
                _lastUpdateBalanceTime = tnow;
                this.UpdateBalanceInternal();
            }

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


        #region account
        public SpotTradeInfo GetOrderAndBalanceStatusForSymbol(string symbol, string paramId = null, long? stime = null)
        {
            if (string.IsNullOrEmpty(symbol) || !Symbols.Contains(symbol)) return null;

            var closedOrders = this.ClosedOrders.Values.Where(o => o.Symbol.Contains(symbol)).ToList();
            if (!string.IsNullOrEmpty(paramId)) closedOrders = closedOrders.Where(o => o.RefId.Contains(paramId)).ToList();
            if (stime.HasValue) closedOrders = closedOrders.Where(o => o.TimeCreated.GetUnixTimeFromUTC() > stime.Value).ToList();

            var boughtOrders = closedOrders.Where(o => o.Ordertype == OrderType.buy_limit && o.DealAmount > 0).ToList();
            var soldOrders = closedOrders.Where(o => o.Ordertype == OrderType.sell_limit && o.DealAmount > 0).ToList();
            //var boughtOrders = closedOrders.Where(o => o.Ordertype == OrderType.buy_limit && o.State != OrderState.cancelled).ToList();
            //var soldOrders = closedOrders.Where(o => o.Ordertype == OrderType.sell_limit && o.State != OrderState.cancelled).ToList();
            //var orders = closedOrders.Where(o => o.State != OrderState.cancelled).ToList();

            var openOrders = this.OpenOrders.Values.Where(o => o.Symbol == symbol);
            if (!string.IsNullOrEmpty(paramId)) openOrders = openOrders.Where(o => o.RefId.Contains(paramId));
            //var standingBuyOrders = openOrders.Where(o => o.Ordertype == OrderType.buy_limit && (o.State == OrderState.open || o.State == OrderState.partial_filled)).ToList();
            //var standingSellOrders = openOrders.Where(o => o.Ordertype == OrderType.sell_limit && (o.State == OrderState.open || o.State == OrderState.partial_filled)).ToList();
            var openBuyOrders = openOrders.Where(o => o.Ordertype == OrderType.buy_limit).ToList();
            var openSellOrders = openOrders.Where(o => o.Ordertype == OrderType.sell_limit).ToList();

            var s = new SpotTradeInfo();
            {
                var order = soldOrders.OrderBy(o => o.TimeLast).LastOrDefault();
                if (order != null)
                {
                    s._prevCreatedTimeSold = order.TimeCreated.GetUnixTimeFromUTC();
                    //s._prevTimeLastSold = order.TimeLast.GetUnixTimeFromUTC();
                    s._prevSoldPrice = order.Price;
                    s._lastOrder = order;
                }
            }

            {
                var order = boughtOrders.OrderBy(o => o.TimeLast).LastOrDefault();
                if (order != null)
                {
                    s._prevCreatedTimeBought = order.TimeCreated.GetUnixTimeFromUTC();
                    s._prevBoughtPrice = order.Price;
                }
            }

            foreach (var o in soldOrders)
            {
                ++s.NumSoldOrder;
                s.NetC1 -= o.DealAmount;
                s.NetC2 += o.DealAmount * o.Price * 0.999;

                s.AvgSoldPrice += o.DealAmount * o.Price;
                s.QtySold += o.DealAmount;
            }

            foreach (var o in boughtOrders)
            {
                ++s.NumBoughtOrder;
                s.NetC1 += o.DealAmount * 0.999;
                s.NetC2 -= o.DealAmount * o.Price;

                s.AvgBoughtPrice += o.DealAmount * o.Price;
                s.QtyBought += o.DealAmount;
            }

            foreach (var o in openSellOrders)
            {
                ++s.NumOpenSellOrder;
                s.HoldC1 += o.Amount - o.DealAmount;
                s.ExptC2 += (o.Amount - o.DealAmount) * o.Price * 0.999;

                s.NetC1 -= o.DealAmount;
                s.NetC2 += o.DealAmount * o.Price * 0.999;

                s.AvgSoldPrice += o.DealAmount * o.Price;
                s.QtySold += o.DealAmount;
            }

            foreach (var o in openBuyOrders)
            {
                ++s.NumOpenSellOrder;

                s.HoldC2 += (o.Amount - o.DealAmount) * o.Price;
                s.ExptC1 += (o.Amount - o.DealAmount) * 0.999;

                s.NetC1 += o.DealAmount * 0.999;
                s.NetC2 -= o.DealAmount * o.Price;

                s.AvgBoughtPrice += o.DealAmount * o.Price;
                s.QtyBought += o.DealAmount;
            }

            if (s.QtySold > 0) s.AvgSoldPrice /= s.QtySold;
            if (s.QtyBought > 0) s.AvgBoughtPrice /= s.QtyBought;

            var c1 = symbol.Split('_')[0];
            var c2 = symbol.Split('_')[1];
            var balance1 = this.GetBalance(c1);
            var balance2 = this.GetBalance(c2);

            if (balance1 != null)
                s.AvailC1 = balance1.Available;
            if (balance2 != null)
                s.AvailC2 = balance2.Available;
            return s;
        }

        public DateTime GetCurrentTime()
        {
            return _isSimulationMode ? _currentTime.GetUTCFromUnixTime() : DateTime.UtcNow;
        }

        public SpotBalance GetBalance(string crypto)
        {
            if (Balances.ContainsKey(crypto)) return new SpotBalance(Balances[crypto]);
            return null;
        }

        public void UpdateBalance(params SpotBalance[] balances)
        {
            if (_isSimulationMode)
            {
                foreach (var balance in balances)
                {
                    if (!Balances.ContainsKey(balance.Currency)) Balances.TryAdd(balance.Currency, new SpotBalance());
                    if (!Balances[balance.Currency].Equals(balance))
                    {
                        Balances[balance.Currency].Copy(balance);
                        OnCurrencyBalanceUpdated?.Invoke(this, new SpotBalance(balance));
                    }
                }
            }
            else if (this.IsStarted)
            {
                foreach (var o in balances) _balanceToUpdate.Enqueue(o);
                this.ProcessBalanceToUpdateQueue();
            }
        }

        private void UpdateBalanceInternal()
        {
            lock (this)
            {
                if (_isProcessBalanceBusy) return;
                _isProcessBalanceBusy = !_isProcessBalanceBusy;
            }

            Task.Run(() =>
            {
                try
                {
                    //this.LogDebug("pull balance");
                    var d = _trade.GetAccountBalance();
                    if (d.Result && d.Data != null)
                    {
                        this.Balances.Clear();
                        foreach (var kv in d.Data)
                            this.Balances.TryAdd(kv.Key, kv.Value);
                        OnAccountBalanceUpdated?.Invoke(this, new List<SpotBalance>(this.Balances.Values));
                    }
                }
                finally
                {
                    _isProcessBalanceBusy = !_isProcessBalanceBusy;
                }
            });
        }

        #endregion

        #region order related
        public List<FZOrder> GetOpenOrders(string symbol)
        {
            var orders = OpenOrders.Values.Where(o => o.Symbol == symbol).ToList();
            return orders;
            //if (orders != null)
            //{
            //    var lst = orders.ConvertAll(o => new FZOrder(o));
            //    return lst;
            //}
            //return null;
        }

        public List<FZOrder> GetClosedOrders(string symbol)
        {
            var orders = ClosedOrders.Values.Where(o => o.Symbol == symbol).ToList();
            return orders;
        }

        public bool IsOrderActionInProgress(string symbol)
        {
            if (!_syncOrderAction.ContainsKey(symbol)) return false;
            return _syncOrderAction[symbol].IsHandlingInProgress(this.GetCurrentTime().GetUnixTimeFromUTC());
        }

        public void SubmitOrder(CommandId commandId, FZOrder order)
        {
            if (!this.IsStarted) return;

            if (_isSimulationMode)
            {
                if (commandId == CommandId.PlaceOrder)
                    this.PlaceOrder(order);
                else if (commandId == CommandId.CancelOrder)
                    this.CancelOrder(order);
            }
            else
            {
                _ordersToSubmit.Enqueue(new ManualCommand<FZOrder> { CommandId = commandId, Order = order });
                this.ProcessOrdersTosubmitQueue();
            }
        }

        public FZOrder PlaceOrder(FZOrder order)
        {
            this.LogDebug($"PlaceOrder: {ConvertOrderToStr(order)}");
            var r = _trade.PlaceOrder(order);
            if (!r.Result)
            {
                this.LogDebug(r.Message);
                return null;
            }
            else
            {
                this.AddRefId(r.Data);
                if (!_isSimulationMode) 
                    Thread.Sleep(50);
                return r.Data;
            }
        }

        public bool CancelOrder(FZOrder order)
        {
            this.LogDebug($"CancelOrder: {ConvertOrderToStr(order)}");
            //if (!_refIdToPrevRefId.ContainsKey(order.RefId))
            //{
            //    _refIdToPrevRefId.Add(order.RefId, order.PrevRefId);
            //    this.SetDumpFile(true);
            //}
            var isStopOrder = order.Ordertype == OrderType.stop_buy || order.Ordertype == OrderType.stop_sell;
            var orderId = isStopOrder ? order.AlgoId : order.OrderId;
            var r = _trade.CancelOrder(order.Symbol, orderId, isStopOrder);
            if (!_isSimulationMode) Thread.Sleep(50);
            this.AddRefId(order);
            return r.Data;
        }

        public void ModifyOrderPrice(string symbol, string orderId, double newPrice)
        {
            this.LogDebug($"ModifyOrderPrice: {symbol} orderId = {orderId} newPrice = {newPrice}");
            var r = _trade.ModifyOrderPrice(symbol, orderId, newPrice);
            if (!_isSimulationMode) Thread.Sleep(50);
        }

        private void AddRefId(FZOrder order)
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
        #endregion

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

        //public void AddOrderIdToUpdateTimeLast(string orderId)
        //{
        //    if (!_orderIdToUpdateTimeLast.Contains(orderId))
        //    {
        //        _orderIdToUpdateTimeLast.Enqueue(orderId);
        //        this.ProcessOrdersToCheckQueue();
        //    }
        //}


        //private void FillExistingOrderInfo(FZOrder o)
        //{
        //    if (_refIdToPrevRefId.ContainsKey(o.RefId)) o.PrevRefId = _refIdToPrevRefId[o.RefId];
        //    //if (_orderIdToTimeCreated.ContainsKey(o.OrderId)) o.TimeCreated = _orderIdToTimeCreated[o.OrderId];
        //    //if (_orderIdToTimeLast.ContainsKey(o.OrderId)) o.TimeLast = _orderIdToTimeLast[o.OrderId];
        //}

        /// <summary>
        /// 1. get open orders from remote 
        /// 2. if exist in local memory => update => done by checkOrder
        /// 3. if not exist in local memory => add => done by checkOrder
        /// 4. if local order not exist in remote orders => remove from open order list => done by chckOrder in update closed orders
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isTriggerEvent"></param>
        private void UpdteOpenOrdersFromServer(string symbol, bool isTriggerEvent, bool isLog=true)
        {
            var r1 = _trade.GetOpenOrdersBySymbol(symbol);
            if (r1.Result && r1.Data != null)
            {
                foreach (var o in r1.Data)
                    this.CheckOrderLocal(o, isTriggerEvent, isLog);

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
        public void UpdateLastTimeAndCheckOrderLocal(FZOrder order, bool isTriggerEvent)
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

        private bool CheckOrderLocal(FZOrder o, bool isTriggerEvent, bool isLog = true)
        {
            if (o == null) return false;
            lock (this)
            {
                this.RemoveRefId(o);

                //this.UpdateTimeCreatedAndTimeLast(o);
                //_trade.GetLastTimeForOrder()
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
                    if (isTriggerEvent) OnOrderUpdated?.Invoke(this, o);
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (isExistingOpen && !isExistingClose && !isOrderOpen)
                {
                    //if (o.RefId == "SWPHAUSDT36001048eabx4")
                    //    this.LogDebug($"existing open order closed: {ConvertOrderToStr(o)}");
                    this.OpenOrders[o.OrderId].Copy(o);
                    if (isTriggerEvent) OnOrderClosed?.Invoke(this, o);

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
                    if (isTriggerEvent) OnOrderUpdated?.Invoke(this, o);
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (!isExistingOpen && !isExistingClose && isOrderOpen)
                {
                    if (isLog) this.LogDebug($"new order opened: {ConvertOrderToStr(o)}");
                    if (isTriggerEvent) this.OnOrderOpened?.Invoke(this, o);
                    if (this.OpenOrders.TryAdd(o.OrderId, o))
                    {
                        if (isLog) this.LogDebug($"new order added to open order list: {ConvertOrderToStr(o)}");
                        //if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
                        if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, null);
                    }
                    isUpdated = true;
                    this.SetDumpFile(true);
                }
                else if (!isExistingOpen && !isExistingClose && !isOrderOpen)
                {
                    this.LogDebug($"missed order closed: {ConvertOrderToStr(o)}");
                    if (isTriggerEvent) OnOrderClosed?.Invoke(this, o);

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
        void ProcessBalanceToUpdateQueue()
        {
            lock (this)
            {
                if (_isProcessBalanceBusy) return;
                _isProcessBalanceBusy = true;
            }

            Task.Run(() =>
            {
                try
                {
                    SpotBalance balance;
                    while (_balanceToUpdate.TryDequeue(out balance))
                    {
                        if (!Balances.ContainsKey(balance.Currency)) Balances.TryAdd(balance.Currency, new SpotBalance());
                        if (!Balances[balance.Currency].Equals(balance))
                        {
                            Balances[balance.Currency].Copy(balance);
                            OnCurrencyBalanceUpdated?.Invoke(this, new SpotBalance(balance));
                        }
                    }
                }
                finally
                {
                    _isProcessBalanceBusy = false;
                }
            });
        }

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

                    //string orderId;
                    //while (_orderIdToUpdateTimeLast.TryDequeue(out orderId))
                    //{
                    //    if (this.ClosedOrders.ContainsKey(orderId))
                    //    {
                    //        var order = this.ClosedOrders[orderId];
                    //        this.CheckOrderRemote(o, true);

                    //        ++count;
                    //        if (count > 10)
                    //        {
                    //            if (!_isSimulationMode) Thread.Sleep(2000);
                    //            count = 0;
                    //        }
                    //    }
                    //}
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

        void ProcessOrdersTosubmitQueue()
        {
            lock (this)
            {
                if (_isProcessOrdersTosubmitBusy) return;
                _isProcessOrdersTosubmitBusy = true;
            }

            Task.Run(() =>
            {
                try
                {
                    ManualCommand<FZOrder> command;
                    while (_ordersToSubmit.TryDequeue(out command))
                    {
                        if (command.CommandId == CommandId.PlaceOrder)
                            this.PlaceOrder(command.Order);
                        else if (command.CommandId == CommandId.CancelOrder)
                            this.CancelOrder(command.Order);

                        if (!_isSimulationMode) Thread.Sleep(10);
                    }
                }
                finally
                {
                    _isProcessOrdersTosubmitBusy = false;
                }
            });
        }
        #endregion


        #region start/stop/initialize/pull data
        public async Task StartAsync()
        {
            if (!_isSimulationMode)
            {
                if (!IsStarted)
                {
                    await this.Initialize().ConfigureAwait(true);
                    _timer1.Interval = _timerinterval;
                    _timer1.Elapsed += _timer1_Elapsed;
                    _timer1.Start();
                    IsStarted = true;
                }
            }
            else
            {
                //foreach (var key in _leverages.Keys)
                //{
                //    var crypto = key.Split('-')[0];
                //    Balances.TryAdd(crypto, new SpotBalance { Currency = crypto, Available = 0 });
                //    _tickers.TryAdd(key, new Ticker());
                //}
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

        public async Task Initialize()
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
                    this.UpdteOpenOrdersFromServer(s, false, false);
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

                //initialize balances
                await Task.Run(() =>
                {
                    var d = _trade.GetAccountBalance();
                    if (d.Result && d.Data != null)
                    {
                        Balances.Clear();
                        foreach (var kv in d.Data)
                            Balances.TryAdd(kv.Key, kv.Value);
                    }
                }).ConfigureAwait(true);
            }
            finally
            {
                _isProcessBalanceBusy = _isProcessOrdersToCheckQueueBusy = _isProcessOrdersTosubmitBusy = false;
            }
        }

        public void UpdateTime(long time)
        {
            _currentTime = time;
        }

        public void DumpToFile()
        {
            lock (this)
            {
                var delimiter = "\nqxdjt394";
                var str = new StringBuilder();
                var datestr = DateTime.UtcNow.ToString("yyyyMMdd");
                var datapath = Path.GetDirectoryName(_fileNameDump);

                str.Append(JsonConvert.SerializeObject(OpenOrders, Formatting.Indented) + delimiter);
                str.Append(JsonConvert.SerializeObject(ClosedOrders, Formatting.Indented) + delimiter);
                //str.Append(JsonConvert.SerializeObject(_refIdToPrevRefId, Formatting.Indented) + delimiter);
                //str.Append(JsonConvert.SerializeObject(_orderIdToTimeLast, Formatting.Indented) + delimiter);
                //str.Append(JsonConvert.SerializeObject(_orderIdToTimeCreated, Formatting.Indented));

                File.WriteAllText(_fileNameDump, str.ToString());
                File.WriteAllText(Path.Combine(datapath, $"{datestr}_{Path.GetFileName(_fileNameDump)}"), str.ToString());

                //using (var stream = new MemoryStream())
                //{
                //    ObjectSerializer.SerializeObject(stream, OpenOrders);
                //    ObjectSerializer.SerializeObject(stream, ClosedOrders);
                //    ObjectSerializer.SerializeObject(stream, _refIdToPrevRefId);
                //    ObjectSerializer.SerializeObject(stream, _orderIdToTimeLast);
                //    ObjectSerializer.SerializeObject(stream, _orderIdToTimeCreated);
                //    BinarySerializer.WriteMemoryStreamToFile(FileNameDump, stream);
                //}
            }
        }

        public void LoadFromFile()
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
                        //var s = this.GetOrderAndBalanceStatusForSymbol("LTC_ETH", "LTCETH36001000");
                        //s = this.GetOrderAndBalanceStatusForSymbol("MCO_ETH", "MCOETH3600090");

                        //_refIdToPrevRefId = JsonConvert.DeserializeObject<Dictionary<string, string>>(items[2]);
                        //_orderIdToTimeLast = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(items[3]);
                        //_orderIdToTimeCreated = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(items[4]);
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
        void LogDebug(string str)
        {
            if (_isEnableLog)
                Log.Debug("OkexSpotProxy: " + str);
        }

        public static string ConvertOrderToStr(FZOrder o)
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

    //public class SpotProxy : CINotification, ISpotProxy
    //{
    //    static DateTime _compareTime = DateTime.Parse("2019/01/01");

    //    private ISpotTradeService _trade;
    //    private ConcurrentDictionary<string, SyncStatus> _syncOrderAction;

    //    //string _exchange;
    //    //string _symbol;
    //    //private ICryptoCaptureService _capture;  
    //    //private string _filenameClosedOrders;
    //    //private string _filenameOpenOrders;
    //    //private string _filenameOrderToSubmit;
    //    //private string _filenameOrderToCheck;
    //    //private string _filenameRefIdToPrevRefId;
    //    //private string _filenameOrderIdToTimeLast;
    //    //private string _filenameOrderIdToTimeCreated;

    //    private bool _isNeedSaveOrders = false;
    //    private bool _isLoadAndDumpFileBusy = false;
    //    private bool _isProcessOrdersTosubmitBusy = false;
    //    private bool _isProcessOrdersToCheckQueueBusy = false;
    //    private bool _isProcessBalanceBusy = false;
    //    private bool _isProcessPullOrderBusy = false;

    //    private System.Timers.Timer _timer1 = new System.Timers.Timer();
    //    private bool _IsStarted = false;
    //    public bool IsStarted
    //    {
    //        get { return _IsStarted; }
    //        set
    //        {
    //            if (_IsStarted != value)
    //            {
    //                _IsStarted = value; OnStateChanged?.Invoke(this, value);
    //            }
    //        }
    //    }

    //    public List<string> Symbols { get; private set; }
    //    //private List<string> _orderIdsToCheck = new List<string>();
    //    //private ConcurrentDictionary<string, Ticker> _tickers = new ConcurrentDictionary<string, Ticker>();
    //    //private ConcurrentDictionary<string, int> _leverages = new ConcurrentDictionary<string, int>();
    //    //private Dictionary<string, DateTime> _orderIdToTimeCreated = new Dictionary<string, DateTime>();
    //    //private Dictionary<string, DateTime> _orderIdToTimeLast = new Dictionary<string, DateTime>();

    //    public ConcurrentDictionary<string, SpotBalance> Balances { get; private set; } = new ConcurrentDictionary<string, SpotBalance>();
    //    public ConcurrentDictionary<string, FZOrder> OpenOrders { get; private set; } = new ConcurrentDictionary<string, FZOrder>();
    //    public ConcurrentDictionary<string, FZOrder> ClosedOrders { get; private set; } = new ConcurrentDictionary<string, FZOrder>();

    //    //private Dictionary<string, string> _refIdToPrevRefId = new Dictionary<string, string>();
    //    private ConcurrentQueue<ManualCommand<FZOrder>> _ordersToSubmit = new ConcurrentQueue<ManualCommand<FZOrder>>();

    //    //private ConcurrentQueue<string> _orderIdToUpdateTimeLast = new ConcurrentQueue<string>();
    //    private ConcurrentQueue<FZOrder> _orderToCheck = new ConcurrentQueue<FZOrder>();
    //    private ConcurrentQueue<SpotBalance> _balanceToUpdate = new ConcurrentQueue<SpotBalance>();
    //    private ConcurrentQueue<string> _queuePullOrder = new ConcurrentQueue<string>();

    //    //private IRateGate _gate = new RateGate(20, new TimeSpan(0, 0, 2));;         //in simulation mode we remove gate check
    //    private bool _isSimulationMode;  //in simulation mode we does not start timer loop to check order status, alternatviely check order status is triggered externally
    //    private double _timerinterval;
    //    private long _lastUpdateBalanceTime;

    //    //long _lastUpdateOrderTime;
    //    private ConcurrentDictionary<string, long> _lastUpdateOrderTimePerSymbol = new ConcurrentDictionary<string, long>();
    //    private int _pollOrderIntervalPerSymbol = 60;
    //    private int _pollBalanceInterval = 60;
    //    private long _currentTime;
    //    private string _fileNameDump;
    //    private bool _isEnableLog;

    //    //private HashSet<string> _checkedOrderIds = new HashSet<string>();

    //    //private ConcurrentDictionary<string, long> _lastUpdateTime = new ConcurrentDictionary<string, long>();
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderOpened;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderClosed;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderUpdated;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<FZOrder> OnOrderCancelled;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnOpenOrderListChanged;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<FZOrder>> OnClosedOrderListChanged;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<SpotBalance> OnCurrencyBalanceUpdated;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<SpotBalance>> OnAccountBalanceUpdated;
    //    public event CryptoCommon.EventHandlers.StateChangedEventHandler OnStateChanged;
    //    public event CryptoCommon.EventHandlers.ExceptionOccuredEventHandler OnExceptionOccured;

    //    public SpotProxy(ISpotTradeService trade, List<string> symbols, string dumpfile, int timerInterval, bool isSimuationMode, bool isEnableLog)
    //    {
    //        _trade = trade;
    //        _isSimulationMode = isSimuationMode;
    //        _timerinterval = timerInterval;
    //        _isEnableLog = isEnableLog;

    //        Func<long> fun = () => this.GetCurrentTime().GetUnixTimeFromUTC();
    //        _syncOrderAction = new ConcurrentDictionary<string, SyncStatus>(); //new SyncStatus(300, fun);

    //        this.Symbols = symbols;
    //        var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
    //        //while (symbols.Count * _pollOrderIntervalPerSymbol < 600)  ++_pollOrderIntervalPerSymbol;
    //        //while (symbols.Count * _pollOrderIntervalPerSymbol > 3600) --_pollOrderIntervalPerSymbol;
    //        //if (_pollOrderIntervalPerSymbol < 60) _pollOrderIntervalPerSymbol = 60;
    //        //var rnd = new Random();
    //        //var delta = _pollServerIntervalPerSymbol;
    //        for (int i = 0; i < symbols.Count; i++)
    //        {
    //            //var t = rnd.Next(0, _pollServerInterval);
    //            _lastUpdateOrderTimePerSymbol.TryAdd(symbols[i], tnow + 120 + i * _pollOrderIntervalPerSymbol);
    //        }
    //        _lastUpdateBalanceTime = tnow;
    //        _fileNameDump = dumpfile;

    //        //_fileNameDump = Path.Combine(datapath, "orderproxy-spot.dump");
    //        //_filenameOpenOrders = Path.Combine(datapath, "spotOpenOrders.json");
    //        //_filenameClosedOrders = Path.Combine(datapath, "spotClosedOrders.json");
    //        //_filenameOrderToSubmit = Path.Combine(datapath, "spotOrderToSubmit.json");
    //        //_filenameOrderToCheck = Path.Combine(datapath, "spotOrderToCheck.json");
    //        //_filenameRefIdToPrevRefId = Path.Combine(datapath, "spotRefIdToPrevId.json");
    //        //_filenameOrderIdToTimeLast = Path.Combine(datapath, "spotOrderIdToTimeLast.json");
    //        //_filenameOrderIdToTimeCreated = Path.Combine(datapath, "spotOrderIdToTimeCreated.json");
    //        //this.PropertyChanged += OrderManager_PropertyChanged;
    //        //this.OnOpenOrderListChanged += (object sender, IList<FZOrder> orders) => this.LogDebug($"order  added: {order.RefId} {OpenOrders[orderId].OrderId} {OpenOrders[orderId].Symbol}");
    //    }

    //    private void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    //    {
    //        var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
    //        if (tnow - _lastUpdateBalanceTime > _pollBalanceInterval)
    //        {
    //            _lastUpdateBalanceTime = tnow;
    //            this.UpdateBalanceInternal();
    //        }

    //        foreach (var symbol in Symbols)
    //        {
    //            if (tnow - _lastUpdateOrderTimePerSymbol[symbol] >= _pollOrderIntervalPerSymbol)
    //            {
    //                _lastUpdateOrderTimePerSymbol[symbol] = tnow;
    //                if (!_queuePullOrder.Contains(symbol))
    //                {
    //                    _queuePullOrder.Enqueue(symbol);
    //                    this.ProcessUpdateOrderQueue();
    //                }
    //            }
    //        }

    //        if (_isNeedSaveOrders)
    //        {
    //            lock (this)
    //            {
    //                if (_isLoadAndDumpFileBusy) return;
    //                _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
    //            }

    //            try
    //            {
    //                this.DumpToFile();
    //            }
    //            catch (Exception ex)
    //            {
    //                this.LogDebug("Ignore dumping error!");
    //                this.LogDebug(ex.ToString());
    //            }
    //            finally
    //            {
    //                this.SetDumpFile(false);
    //                _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
    //            }
    //        }
    //    }


    //    #region account
    //    public SpotTradeInfo GetOrderAndBalanceStatusForSymbol(string symbol, string paramId = null, long? stime = null)
    //    {
    //        if (string.IsNullOrEmpty(symbol) || !Symbols.Contains(symbol)) return null;

    //        var closedOrders = this.ClosedOrders.Values.Where(o => o.Symbol.Contains(symbol)).ToList();
    //        if (!string.IsNullOrEmpty(paramId)) closedOrders = closedOrders.Where(o => o.RefId.Contains(paramId)).ToList();
    //        if (stime.HasValue) closedOrders = closedOrders.Where(o => o.TimeCreated.GetUnixTimeFromUTC() > stime.Value).ToList();

    //        var boughtOrders = closedOrders.Where(o => o.Ordertype == OrderType.buy_limit && o.DealAmount > 0).ToList();
    //        var soldOrders = closedOrders.Where(o => o.Ordertype == OrderType.sell_limit && o.DealAmount > 0).ToList();
    //        //var boughtOrders = closedOrders.Where(o => o.Ordertype == OrderType.buy_limit && o.State != OrderState.cancelled).ToList();
    //        //var soldOrders = closedOrders.Where(o => o.Ordertype == OrderType.sell_limit && o.State != OrderState.cancelled).ToList();
    //        //var orders = closedOrders.Where(o => o.State != OrderState.cancelled).ToList();

    //        var openOrders = this.OpenOrders.Values.Where(o => o.Symbol == symbol);
    //        if (!string.IsNullOrEmpty(paramId)) openOrders = openOrders.Where(o => o.RefId.Contains(paramId));
    //        //var standingBuyOrders = openOrders.Where(o => o.Ordertype == OrderType.buy_limit && (o.State == OrderState.open || o.State == OrderState.partial_filled)).ToList();
    //        //var standingSellOrders = openOrders.Where(o => o.Ordertype == OrderType.sell_limit && (o.State == OrderState.open || o.State == OrderState.partial_filled)).ToList();
    //        var openBuyOrders = openOrders.Where(o => o.Ordertype == OrderType.buy_limit).ToList();
    //        var openSellOrders = openOrders.Where(o => o.Ordertype == OrderType.sell_limit).ToList();

    //        var s = new SpotTradeInfo();
    //        {
    //            var order = soldOrders.OrderBy(o => o.TimeLast).LastOrDefault();
    //            if (order != null)
    //            {
    //                s._prevCreatedTimeSold = order.TimeCreated.GetUnixTimeFromUTC();
    //                //s._prevTimeLastSold = order.TimeLast.GetUnixTimeFromUTC();
    //                s._prevSoldPrice = order.Price;
    //                s._lastOrder = order;
    //            }
    //        }

    //        {
    //            var order = boughtOrders.OrderBy(o => o.TimeLast).LastOrDefault();
    //            if (order != null)
    //            {
    //                s._prevCreatedTimeBought = order.TimeCreated.GetUnixTimeFromUTC();
    //                s._prevBoughtPrice = order.Price;
    //            }
    //        }

    //        foreach (var o in soldOrders)
    //        {
    //            ++s.NumSoldOrder;
    //            s.NetC1 -= o.DealAmount;
    //            s.NetC2 += o.DealAmount * o.Price * 0.999;

    //            s.AvgSoldPrice += o.DealAmount * o.Price;
    //            s.QtySold += o.DealAmount;
    //        }

    //        foreach (var o in boughtOrders)
    //        {
    //            ++s.NumBoughtOrder;
    //            s.NetC1 += o.DealAmount * 0.999;
    //            s.NetC2 -= o.DealAmount * o.Price;

    //            s.AvgBoughtPrice += o.DealAmount * o.Price;
    //            s.QtyBought += o.DealAmount;
    //        }

    //        foreach (var o in openSellOrders)
    //        {
    //            ++s.NumOpenSellOrder;
    //            s.HoldC1 += o.Amount - o.DealAmount;
    //            s.ExptC2 += (o.Amount - o.DealAmount) * o.Price * 0.999;

    //            s.NetC1 -= o.DealAmount;
    //            s.NetC2 += o.DealAmount * o.Price * 0.999;

    //            s.AvgSoldPrice += o.DealAmount * o.Price;
    //            s.QtySold += o.DealAmount;
    //        }

    //        foreach (var o in openBuyOrders)
    //        {
    //            ++s.NumOpenSellOrder;

    //            s.HoldC2 += (o.Amount - o.DealAmount) * o.Price;
    //            s.ExptC1 += (o.Amount - o.DealAmount) * 0.999;

    //            s.NetC1 += o.DealAmount * 0.999;
    //            s.NetC2 -= o.DealAmount * o.Price;

    //            s.AvgBoughtPrice += o.DealAmount * o.Price;
    //            s.QtyBought += o.DealAmount;
    //        }

    //        if (s.QtySold > 0) s.AvgSoldPrice /= s.QtySold;
    //        if (s.QtyBought > 0) s.AvgBoughtPrice /= s.QtyBought;

    //        var c1 = symbol.Split('_')[0];
    //        var c2 = symbol.Split('_')[1];
    //        var balance1 = this.GetBalance(c1);
    //        var balance2 = this.GetBalance(c2);

    //        if (balance1 != null)
    //            s.AvailC1 = balance1.Available;
    //        if (balance2 != null)
    //            s.AvailC2 = balance2.Available;
    //        return s;
    //    }

    //    public DateTime GetCurrentTime()
    //    {
    //        return _isSimulationMode ? _currentTime.GetUTCFromUnixTime() : DateTime.UtcNow;
    //    }

    //    public SpotBalance GetBalance(string crypto)
    //    {
    //        if (Balances.ContainsKey(crypto)) return new SpotBalance(Balances[crypto]);
    //        return null;
    //    }

    //    public void UpdateBalance(params SpotBalance[] balances)
    //    {
    //        if (_isSimulationMode)
    //        {
    //            foreach (var balance in balances)
    //            {
    //                if (!Balances.ContainsKey(balance.Currency)) Balances.TryAdd(balance.Currency, new SpotBalance());
    //                if (!Balances[balance.Currency].Equals(balance))
    //                {
    //                    Balances[balance.Currency].Copy(balance);
    //                    OnCurrencyBalanceUpdated?.Invoke(this, new SpotBalance(balance));
    //                }
    //            }
    //        }
    //        else if (this.IsStarted)
    //        {
    //            foreach (var o in balances) _balanceToUpdate.Enqueue(o);
    //            this.ProcessBalanceToUpdateQueue();
    //        }
    //    }

    //    private void UpdateBalanceInternal()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessBalanceBusy) return;
    //            _isProcessBalanceBusy = !_isProcessBalanceBusy;
    //        }

    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                //this.LogDebug("pull balance");
    //                var d = _trade.GetAccountBalance();
    //                if (d.Result && d.Data != null)
    //                {
    //                    this.Balances.Clear();
    //                    foreach (var kv in d.Data)
    //                        this.Balances.TryAdd(kv.Key, kv.Value);
    //                    OnAccountBalanceUpdated?.Invoke(this, new List<SpotBalance>(this.Balances.Values));
    //                }
    //            }
    //            finally
    //            {
    //                _isProcessBalanceBusy = !_isProcessBalanceBusy;
    //            }
    //        });
    //    }

    //    #endregion

    //    #region order related
    //    public List<FZOrder> GetOpenOrders(string symbol)
    //    {
    //        var orders = OpenOrders.Values.Where(o => o.Symbol == symbol).ToList();
    //        return orders;
    //        //if (orders != null)
    //        //{
    //        //    var lst = orders.ConvertAll(o => new FZOrder(o));
    //        //    return lst;
    //        //}
    //        //return null;
    //    }

    //    public List<FZOrder> GetClosedOrders(string symbol)
    //    {
    //        var orders = ClosedOrders.Values.Where(o => o.Symbol == symbol).ToList();
    //        return orders;
    //    }

    //    public bool IsOrderActionInProgress(string symbol)
    //    {
    //        if (!_syncOrderAction.ContainsKey(symbol)) return false;
    //        return _syncOrderAction[symbol].IsHandlingInProgress(this.GetCurrentTime().GetUnixTimeFromUTC());
    //    }

    //    public void SubmitOrder(CommandId commandId, FZOrder order)
    //    {
    //        if (!this.IsStarted) return;

    //        if (_isSimulationMode)
    //        {
    //            if (commandId == CommandId.PlaceOrder)
    //                this.PlaceOrder(order);
    //            else if (commandId == CommandId.CancelOrder)
    //                this.CancelOrder(order);
    //        }
    //        else
    //        {
    //            _ordersToSubmit.Enqueue(new ManualCommand<FZOrder> { CommandId = commandId, Order = order });
    //            this.ProcessOrdersTosubmitQueue();
    //        }
    //    }

    //    public void PlaceOrder(FZOrder order)
    //    {
    //        this.LogDebug($"PlaceOrder: {ConvertOrderToStr(order)}");
    //        var r = _trade.PlaceOrder(order);
    //        this.AddRefId(order);
    //        if (!_isSimulationMode) Thread.Sleep(50);

    //        //if (order.Ordertype == OrderType.buy_limit || order.Ordertype == OrderType.sell_limit)
    //        //    return r.Data.OrderId;
    //        //if (order.Ordertype == OrderType.stop_buy || order.Ordertype == OrderType.stop_sell)
    //        //    return r.Data.AlgoId;
    //        //return null;
    //    }

    //    public void CancelOrder(FZOrder order)
    //    {
    //        this.LogDebug($"CancelOrder: {ConvertOrderToStr(order)}");
    //        //if (!_refIdToPrevRefId.ContainsKey(order.RefId))
    //        //{
    //        //    _refIdToPrevRefId.Add(order.RefId, order.PrevRefId);
    //        //    this.SetDumpFile(true);
    //        //}
    //        var isStopOrder = order.Ordertype == OrderType.stop_buy || order.Ordertype == OrderType.stop_sell;
    //        var orderId = isStopOrder ? order.AlgoId : order.OrderId;
    //        var r = _trade.CancelOrder(order.Symbol, orderId, isStopOrder);
    //        if (!_isSimulationMode) Thread.Sleep(50);
    //        this.AddRefId(order);
    //    }

    //    public void ModifyOrderPrice(string symbol, string orderId, double newPrice)
    //    {
    //        this.LogDebug($"ModifyOrderPrice: {symbol} orderId = {orderId} newPrice = {newPrice}");
    //        var r = _trade.ModifyOrderPrice(symbol, orderId, newPrice);
    //        if (!_isSimulationMode) Thread.Sleep(50);
    //    }

    //    private void AddRefId(FZOrder order)
    //    {
    //        if (!_syncOrderAction.ContainsKey(order.Symbol))
    //        {
    //            Func<long> fun = () => this.GetCurrentTime().GetUnixTimeFromUTC();
    //            var s = new SyncStatus(300, fun);
    //            _syncOrderAction.TryAdd(order.Symbol, s);
    //        }
    //        _syncOrderAction[order.Symbol].AddRefId(order.RefId);
    //    }

    //    private void RemoveRefId(FZOrder order)
    //    {
    //        if (_syncOrderAction.ContainsKey(order.Symbol))
    //            _syncOrderAction[order.Symbol].RemoveRefId(order.RefId);
    //    }
    //    #endregion

    //    #region update orders
    //    public void UpdateOrders(params FZOrder[] orders)
    //    {
    //        if (!this.IsStarted) return;

    //        if (_isSimulationMode)
    //        {
    //            foreach (var o in orders) this.CheckOrderLocal(o, true);
    //        }
    //        else
    //        {
    //            foreach (var o in orders)
    //            {
    //                this.LogDebug($"new order received from server: {ConvertOrderToStr(o)}");
    //                this.RemoveRefId(o);
    //                _orderToCheck.Enqueue(o);
    //            }
    //            this.ProcessOrdersToCheckQueue();
    //        }
    //    }

    //    //public void AddOrderIdToUpdateTimeLast(string orderId)
    //    //{
    //    //    if (!_orderIdToUpdateTimeLast.Contains(orderId))
    //    //    {
    //    //        _orderIdToUpdateTimeLast.Enqueue(orderId);
    //    //        this.ProcessOrdersToCheckQueue();
    //    //    }
    //    //}


    //    //private void FillExistingOrderInfo(FZOrder o)
    //    //{
    //    //    if (_refIdToPrevRefId.ContainsKey(o.RefId)) o.PrevRefId = _refIdToPrevRefId[o.RefId];
    //    //    //if (_orderIdToTimeCreated.ContainsKey(o.OrderId)) o.TimeCreated = _orderIdToTimeCreated[o.OrderId];
    //    //    //if (_orderIdToTimeLast.ContainsKey(o.OrderId)) o.TimeLast = _orderIdToTimeLast[o.OrderId];
    //    //}

    //    /// <summary>
    //    /// 1. get open orders from remote 
    //    /// 2. if exist in local memory => update => done by checkOrder
    //    /// 3. if not exist in local memory => add => done by checkOrder
    //    /// 4. if local order not exist in remote orders => remove from open order list => done by chckOrder in update closed orders
    //    /// </summary>
    //    /// <param name="symbol"></param>
    //    /// <param name="isTriggerEvent"></param>
    //    private void UpdteOpenOrdersFromServer(string symbol, bool isTriggerEvent)
    //    {
    //        var r1 = _trade.GetOpenOrdersBySymbol(symbol);
    //        if (r1.Result && r1.Data != null)
    //        {
    //            foreach (var o in r1.Data)
    //                this.CheckOrderLocal(o, isTriggerEvent);

    //            var d = r1.Data.ToDictionary(o => o.OrderId, o => o);
    //            var openOrders = this.OpenOrders.Values.Where(o => o.Symbol == symbol);
    //            foreach (var o in openOrders)
    //            {
    //                if (!d.ContainsKey(o.OrderId))
    //                    this.UpdateLastTimeAndCheckOrderLocal(o, isTriggerEvent);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// 1. exist in local open orders
    //    /// 2. not exist in local closed orders
    //    /// 3. exist in local closed orders by TimeLast not correct
    //    /// </summary>
    //    /// <param name="symbol"></param>
    //    /// <param name="isTriggerEvent"></param>
    //    private void UpdateCloseOrdersFromServer(string symbol, bool isTriggerEvent)
    //    {
    //        //this.LogDebug($"update closed orders for {symbol}");
    //        var count = 0;
    //        var r2 = _trade.GetClosedOrdersBySymbol(symbol, false);
    //        if (r2.Result && r2.Data != null)
    //        {
    //            foreach (var o in r2.Data)
    //            {
    //                if ((this.OpenOrders.ContainsKey(o.OrderId)) ||
    //                    (!this.ClosedOrders.ContainsKey(o.OrderId)) ||
    //                    (this.ClosedOrders.ContainsKey(o.OrderId) && this.ClosedOrders[o.OrderId].TimeLast <= this.ClosedOrders[o.OrderId].TimeCreated))
    //                {
    //                    //if (!this.ClosedOrders.ContainsKey(o.OrderId))
    //                    //    this.ClosedOrders.TryAdd(o.OrderId, o);
    //                    var r = _trade.GetLastTimeForOrder(o.OrderId, o.Symbol);
    //                    if (!_isSimulationMode) Thread.Sleep(50);
    //                    if (r.Result && o.TimeLast <= (r.Data + 10).GetUTCFromUnixTime())
    //                    {
    //                        o.TimeLast = r.Data.GetUTCFromUnixTime();
    //                        if (this.ClosedOrders.ContainsKey(o.OrderId))
    //                        {
    //                            //this.LogDebug("update last time directly");
    //                            //this.LogDebug($"TimeLast updated for closed order: {ConvertOrderToStr(o)}");
    //                            this.ClosedOrders[o.OrderId].TimeLast = r.Data.GetUTCFromUnixTime();
    //                            this.SetDumpFile(true);
    //                        }
    //                        this.CheckOrderLocal(o, isTriggerEvent);
    //                    }

    //                    ++count;
    //                    if (count > 20)
    //                    {
    //                        count = 0;
    //                        if (!_isSimulationMode) Thread.Sleep(2000);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// for closed order, TimeLast is not correct. trade.CheckOrder will update the TimeLast from server
    //    /// </summary>
    //    /// <param name="order"></param>
    //    /// <param name="isTriggerEvent"></param>
    //    public void UpdateLastTimeAndCheckOrderLocal(FZOrder order, bool isTriggerEvent)
    //    {
    //        if (order != null)
    //        {
    //            var isStopOrder = order.Ordertype == OrderType.stop_buy || order.Ordertype == OrderType.stop_sell;
    //            var r = _trade.CheckOrder(order.Symbol, order.OrderId, isStopOrder);
    //            if (!_isSimulationMode) Thread.Sleep(50);
    //            if (r.Result && r.Data != null)
    //                this.CheckOrderLocal(r.Data, isTriggerEvent);
    //        }
    //    }

    //    private bool CheckOrderLocal(FZOrder o, bool isTriggerEvent)
    //    {
    //        if (o == null) return false;
    //        lock (this)
    //        {
    //            this.RemoveRefId(o);

    //            //this.UpdateTimeCreatedAndTimeLast(o);
    //            //_trade.GetLastTimeForOrder()

    //            var isUpdated = false;
    //            var isExistingOpen = OpenOrders.ContainsKey(o.OrderId);
    //            var isExistingClose = ClosedOrders.ContainsKey(o.OrderId);
    //            var isNewOrder = !isExistingOpen && !isExistingClose;
    //            var isOrderChanged = isExistingOpen ? !OpenOrders[o.OrderId].Equals(o) : (isExistingClose ? !ClosedOrders[o.OrderId].Equals(o) : false);
    //            var isOrderOpen = o.IsOrderOpen();

    //            //existOpen  existClose  isOpen  isChanged
    //            //Y          N           Y       Y          => existing open order updated
    //            //Y          N           N       -          => existing open order closed
    //            //N          Y           Y       -          => invalid
    //            //N          Y           N       Y          => existing order updated
    //            //N          N           Y       -          => new order opened
    //            //N          N           N       -          => missed order closed
    //            if (isExistingOpen && !isExistingClose && isOrderOpen && isOrderChanged)
    //            {
    //                this.LogDebug($"existing open order updated: {ConvertOrderToStr(o)}");
    //                this.OpenOrders[o.OrderId].Copy(o);
    //                if (isTriggerEvent) OnOrderUpdated(this, o);
    //                isUpdated = true;
    //                this.SetDumpFile(true);
    //            }
    //            else if (isExistingOpen && !isExistingClose && !isOrderOpen)
    //            {
    //                //if (o.RefId == "SWPHAUSDT36001048eabx4")
    //                //    this.LogDebug($"existing open order closed: {ConvertOrderToStr(o)}");

    //                this.OpenOrders[o.OrderId].Copy(o);
    //                if (isTriggerEvent) OnOrderClosed(this, o);

    //                FZOrder order;
    //                if (OpenOrders.TryRemove(o.OrderId, out order))
    //                {
    //                    //if (o.OrderId == "orderId_10")
    //                    //    Console.WriteLine();

    //                    this.LogDebug($"order removed from open order list: {ConvertOrderToStr(o)}");
    //                    //if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
    //                    if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, null);
    //                }
    //                if (ClosedOrders.TryAdd(o.OrderId, o))
    //                {
    //                    this.LogDebug($"order added to closed order list: {ConvertOrderToStr(o)}");
    //                    //if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
    //                    if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, null);
    //                }
    //                isUpdated = true;
    //                this.SetDumpFile(true);
    //            }
    //            else if (!isExistingOpen && isExistingClose && isOrderOpen)
    //            {
    //                this.LogDebug($"order exist in close list while it is still open: {ConvertOrderToStr(o)}");
    //                FZOrder order;
    //                if (ClosedOrders.TryRemove(o.OrderId, out order))
    //                    //if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
    //                    if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, null);

    //                if (OpenOrders.TryAdd(o.OrderId, o))
    //                {
    //                    if (isTriggerEvent)
    //                    {
    //                        this.OnOrderOpened?.Invoke(this, o);
    //                        //this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
    //                        this.OnOpenOrderListChanged?.Invoke(this, null);
    //                    }
    //                }
    //                isUpdated = true;
    //                this.SetDumpFile(true);
    //            }
    //            else if (!isExistingOpen && isExistingClose && !isOrderOpen && isOrderChanged)
    //            {
    //                this.LogDebug($"existing closed order updated: {ConvertOrderToStr(o)}");
    //                this.ClosedOrders[o.OrderId].Copy(o);
    //                if (isTriggerEvent) OnOrderUpdated(this, o);
    //                isUpdated = true;
    //                this.SetDumpFile(true);
    //            }
    //            else if (!isExistingOpen && !isExistingClose && isOrderOpen)
    //            {
    //                //this.LogDebug($"new order opened: {ConvertOrderToStr(o)}");
    //                if (isTriggerEvent) this.OnOrderOpened?.Invoke(this, o);
    //                if (this.OpenOrders.TryAdd(o.OrderId, o))
    //                {
    //                    this.LogDebug($"new order added to open order list: {ConvertOrderToStr(o)}");
    //                    //if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
    //                    if (isTriggerEvent) this.OnOpenOrderListChanged?.Invoke(this, null);
    //                }
    //                isUpdated = true;
    //                this.SetDumpFile(true);
    //            }
    //            else if (!isExistingOpen && !isExistingClose && !isOrderOpen)
    //            {
    //                //this.LogDebug($"missed order closed: {ConvertOrderToStr(o)}");
    //                if (isTriggerEvent) OnOrderClosed(this, o);

    //                if (ClosedOrders.TryAdd(o.OrderId, o))
    //                {
    //                    this.LogDebug($"missed order added to closed order list: {ConvertOrderToStr(o)}");
    //                    //if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(t => new FZOrder(t)));
    //                    if (isTriggerEvent) this.OnClosedOrderListChanged?.Invoke(this, null);
    //                }
    //                isUpdated = true;
    //                this.SetDumpFile(true);
    //            }

    //            //if (!isUpdated && !o.IsOrderOpen())
    //            //{
    //            //    this.LogDebug($"order not updated: {o.Symbol} {o.Ordertype} isInOpenList = {isExistingOpen}, isInCloseList = {isExistingClose}, isOrderChanged = {isOrderChanged}, isOrderOpen = {isOrderOpen}");
    //            //}

    //            return isUpdated;
    //        }
    //    }
    //    #endregion


    //    #region processing queue
    //    void ProcessBalanceToUpdateQueue()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessBalanceBusy) return;
    //            _isProcessBalanceBusy = true;
    //        }

    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                SpotBalance balance;
    //                while (_balanceToUpdate.TryDequeue(out balance))
    //                {
    //                    if (!Balances.ContainsKey(balance.Currency)) Balances.TryAdd(balance.Currency, new SpotBalance());
    //                    if (!Balances[balance.Currency].Equals(balance))
    //                    {
    //                        Balances[balance.Currency].Copy(balance);
    //                        OnCurrencyBalanceUpdated?.Invoke(this, new SpotBalance(balance));
    //                    }
    //                }
    //            }
    //            finally
    //            {
    //                _isProcessBalanceBusy = false;
    //            }
    //        });
    //    }

    //    /// <summary>
    //    /// check individual order from server
    //    /// </summary>
    //    void ProcessOrdersToCheckQueue()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessOrdersToCheckQueueBusy) return;
    //            _isProcessOrdersToCheckQueueBusy = true;
    //        }

    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                FZOrder o;
    //                var count = 0;
    //                while (_orderToCheck.TryDequeue(out o))
    //                {
    //                    if (o.IsOrderOpen())
    //                        this.CheckOrderLocal(o, true);
    //                    else
    //                    {
    //                        this.UpdateLastTimeAndCheckOrderLocal(o, true);
    //                        ++count;
    //                        if (count > 10)
    //                        {
    //                            if (!_isSimulationMode) Thread.Sleep(2000);
    //                            count = 0;
    //                        }
    //                    }
    //                }

    //                //string orderId;
    //                //while (_orderIdToUpdateTimeLast.TryDequeue(out orderId))
    //                //{
    //                //    if (this.ClosedOrders.ContainsKey(orderId))
    //                //    {
    //                //        var order = this.ClosedOrders[orderId];
    //                //        this.CheckOrderRemote(o, true);

    //                //        ++count;
    //                //        if (count > 10)
    //                //        {
    //                //            if (!_isSimulationMode) Thread.Sleep(2000);
    //                //            count = 0;
    //                //        }
    //                //    }
    //                //}
    //            }
    //            finally
    //            {
    //                _isProcessOrdersToCheckQueueBusy = false;
    //            }
    //        });
    //    }

    //    //1. pull openOrders and closeOrders from time to time from server, such that server and local cache are always in sync with each other
    //    void ProcessUpdateOrderQueue()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessPullOrderBusy) return;
    //            _isProcessPullOrderBusy = !_isProcessPullOrderBusy;
    //        }

    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                string symbol;
    //                while (_queuePullOrder.TryDequeue(out symbol))
    //                {
    //                    //////////////////////////////////////////////////////////////////////////////
    //                    ///update open orders
    //                    this.UpdteOpenOrdersFromServer(symbol, true);

    //                    /////////////////////////////////////////////////////////////////////////
    //                    ///update closed orders
    //                    this.UpdateCloseOrdersFromServer(symbol, true);
    //                }
    //            }
    //            finally
    //            {
    //                _isProcessPullOrderBusy = !_isProcessPullOrderBusy;
    //            }
    //        });
    //    }

    //    void ProcessOrdersTosubmitQueue()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessOrdersTosubmitBusy) return;
    //            _isProcessOrdersTosubmitBusy = true;
    //        }

    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                ManualCommand<FZOrder> command;
    //                while (_ordersToSubmit.TryDequeue(out command))
    //                {
    //                    if (command.CommandId == CommandId.PlaceOrder)
    //                        this.PlaceOrder(command.Order);
    //                    else if (command.CommandId == CommandId.CancelOrder)
    //                        this.CancelOrder(command.Order);

    //                    if (!_isSimulationMode) Thread.Sleep(10);
    //                }
    //            }
    //            finally
    //            {
    //                _isProcessOrdersTosubmitBusy = false;
    //            }
    //        });
    //    }
    //    #endregion


    //    #region start/stop/initialize/pull data
    //    public async Task StartAsync()
    //    {
    //        if (!_isSimulationMode)
    //        {
    //            if (!IsStarted)
    //            {
    //                await this.Initialize().ConfigureAwait(true);
    //                _timer1.Interval = _timerinterval;
    //                _timer1.Elapsed += _timer1_Elapsed;
    //                _timer1.Start();
    //                IsStarted = true;
    //            }
    //        }
    //        else
    //        {
    //            //foreach (var key in _leverages.Keys)
    //            //{
    //            //    var crypto = key.Split('-')[0];
    //            //    Balances.TryAdd(crypto, new SpotBalance { Currency = crypto, Available = 0 });
    //            //    _tickers.TryAdd(key, new Ticker());
    //            //}
    //            IsStarted = true;
    //        }
    //    }

    //    public void Stop()
    //    {
    //        if (!_isSimulationMode)
    //        {
    //            if (IsStarted)
    //            {
    //                _timer1.Elapsed -= _timer1_Elapsed;
    //                _timer1.Stop();
    //                IsStarted = false;
    //            }
    //        }
    //        else IsStarted = false;
    //    }

    //    public async Task Initialize()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessOrdersToCheckQueueBusy || _isProcessOrdersTosubmitBusy || _isProcessBalanceBusy) return;
    //            _isProcessBalanceBusy = _isProcessOrdersToCheckQueueBusy = _isProcessOrdersTosubmitBusy = true;
    //        }

    //        try
    //        {
    //            this.LoadFromFile();

    //            ///initialize orders
    //            this.OpenOrders.Clear();
    //            foreach (var s in Symbols)
    //            {
    //                Console.WriteLine($"update orders for {s}...");
    //                this.UpdteOpenOrdersFromServer(s, false);
    //                this.UpdateCloseOrdersFromServer(s, false);
    //            }

    //            var tnow = DateTime.UtcNow;
    //            foreach (var o in this.ClosedOrders.Values)
    //            {
    //                if (o.State == OrderState.fully_filled && o.TimeCreated >= o.TimeLast && (tnow - o.TimeCreated).TotalSeconds < 86400 * 30)
    //                {
    //                    Console.WriteLine($"updating TimeLast for {ConvertOrderToStr(o)}");
    //                    var r = _trade.GetLastTimeForOrder(o.OrderId, o.Symbol);
    //                    if (!_isSimulationMode) Thread.Sleep(105);
    //                    if (r.Result && o.TimeLast < r.Data.GetUTCFromUnixTime())
    //                    {
    //                        Console.WriteLine("TimeLast updated!");
    //                        o.TimeLast = r.Data.GetUTCFromUnixTime();
    //                        this.SetDumpFile(true);
    //                    }
    //                }
    //            }

    //            if (this.OpenOrders.Count > 0)
    //                this.OnOpenOrderListChanged?.Invoke(this, OpenOrders.Values.ToList().ConvertAll(o => new FZOrder(o)));

    //            if (this.ClosedOrders.Count > 0)
    //                this.OnClosedOrderListChanged?.Invoke(this, ClosedOrders.Values.ToList().ConvertAll(o => new FZOrder(o)));

    //            //initialize balances
    //            await Task.Run(() =>
    //            {
    //                var d = _trade.GetAccountBalance();
    //                if (d.Result && d.Data != null)
    //                {
    //                    Balances.Clear();
    //                    foreach (var kv in d.Data)
    //                        Balances.TryAdd(kv.Key, kv.Value);
    //                }
    //            }).ConfigureAwait(true);
    //        }
    //        finally
    //        {
    //            _isProcessBalanceBusy = _isProcessOrdersToCheckQueueBusy = _isProcessOrdersTosubmitBusy = false;
    //        }
    //    }

    //    public void UpdateTime(long time)
    //    {
    //        _currentTime = time;
    //    }

    //    public void DumpToFile()
    //    {
    //        lock (this)
    //        {
    //            var delimiter = "\nqxdjt394";
    //            var str = new StringBuilder();
    //            var datestr = DateTime.UtcNow.ToString("yyyyMMdd");
    //            var datapath = Path.GetDirectoryName(_fileNameDump);

    //            str.Append(JsonConvert.SerializeObject(OpenOrders, Formatting.Indented) + delimiter);
    //            str.Append(JsonConvert.SerializeObject(ClosedOrders, Formatting.Indented) + delimiter);
    //            //str.Append(JsonConvert.SerializeObject(_refIdToPrevRefId, Formatting.Indented) + delimiter);
    //            //str.Append(JsonConvert.SerializeObject(_orderIdToTimeLast, Formatting.Indented) + delimiter);
    //            //str.Append(JsonConvert.SerializeObject(_orderIdToTimeCreated, Formatting.Indented));

    //            File.WriteAllText(_fileNameDump, str.ToString());
    //            File.WriteAllText(Path.Combine(datapath, $"{datestr}_{Path.GetFileName(_fileNameDump)}"), str.ToString());

    //            //using (var stream = new MemoryStream())
    //            //{
    //            //    ObjectSerializer.SerializeObject(stream, OpenOrders);
    //            //    ObjectSerializer.SerializeObject(stream, ClosedOrders);
    //            //    ObjectSerializer.SerializeObject(stream, _refIdToPrevRefId);
    //            //    ObjectSerializer.SerializeObject(stream, _orderIdToTimeLast);
    //            //    ObjectSerializer.SerializeObject(stream, _orderIdToTimeCreated);
    //            //    BinarySerializer.WriteMemoryStreamToFile(FileNameDump, stream);
    //            //}
    //        }
    //    }

    //    public void LoadFromFile()
    //    {
    //        lock (this)
    //        {
    //            try
    //            {
    //                if (File.Exists(this._fileNameDump))
    //                {
    //                    var delimiter = "\nqxdjt394";
    //                    var str = File.ReadAllText(this._fileNameDump);
    //                    var items = Regex.Split(str, delimiter);

    //                    this.OpenOrders = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FZOrder>>(items[0]);
    //                    this.ClosedOrders = JsonConvert.DeserializeObject<ConcurrentDictionary<string, FZOrder>>(items[1]);

    //                    var orders = this.ClosedOrders.Values.Where(o => o.DealAmount > 0 || o.State != OrderState.cancelled).ToList();
    //                    this.ClosedOrders = new ConcurrentDictionary<string, FZOrder>(orders.ToDictionary(o => o.OrderId, o => o));
    //                    //var s = this.GetOrderAndBalanceStatusForSymbol("LTC_ETH", "LTCETH36001000");
    //                    //s = this.GetOrderAndBalanceStatusForSymbol("MCO_ETH", "MCOETH3600090");

    //                    //_refIdToPrevRefId = JsonConvert.DeserializeObject<Dictionary<string, string>>(items[2]);
    //                    //_orderIdToTimeLast = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(items[3]);
    //                    //_orderIdToTimeCreated = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(items[4]);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                //Console.WriteLine(ex.ToString());
    //                this.LogDebug(ex.ToString());
    //            }
    //        }
    //    }
    //    #endregion

    //    #region util
    //    void LogDebug(string str)
    //    {
    //        if (_isEnableLog)
    //            Log.Debug("OkexSpotProxy: " + str);
    //    }

    //    public static string ConvertOrderToStr(FZOrder o)
    //    {
    //        return $"{o.RefId} {o.OrderId} {o.Ordertype} {o.Symbol} price={o.Price} amount={o.Amount} {o.State} {o.TimeCreated} {o.TimeLast}";
    //    }

    //    void SetDumpFile(bool v)
    //    {
    //        lock (this)
    //        {
    //            _isNeedSaveOrders = v;
    //        }
    //    }
    //    #endregion
    //}
}
