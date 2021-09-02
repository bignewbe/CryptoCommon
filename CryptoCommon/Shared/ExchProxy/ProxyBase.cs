using CryptoCommon.DataTypes;
using CryptoCommon.Future;
using CryptoCommon.Future.DataType;
using CryptoCommon.Future.Interface;
using CryptoCommon.Interfaces;
using CryptoCommon.Services;
using CryptoCommon.Shared.ExchProxy;
using Newtonsoft.Json;
using PortableCSharpLib;
using PortableCSharpLib.Facility;
using PortableCSharpLib.Interace;
using PortableCSharpLib.Util;
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

namespace CryptoCommon.Future
{
    public class FutureExchProxy
    {
        IOrderProxy _orderProxy;
        IGenericProxy<FutureBalance> _futureBalanceProxy;
        IGenericProxy<FuturePosition> _futurePositionProxy;
        IGenericProxy<SpotBalance> _spotBalanceProxy;        
        IMarketProxy _marketProxy;
        IFutureAccountProxy _futureAccountProxy;
        ISpotAccountProxy _spotAccountProxy;
        ISpotWebSocket _socket;
    }

    public class SpotExchProxy
    {
        IOrderProxy _orderProxy;
        IGenericProxy<SpotBalance> _balanceProxy;
        ISpotWebSocket _socket;
    }

    //public class ItemListProxy<T> where T : class, IIdEqualCopy<T>
    //{
    //    Func<string, List<T>> _funcGetItems;

    //    private bool _isDumpFile => !string.IsNullOrEmpty(_fileNameDump);
    //    private bool _isProcessItemBusy = false;
    //    private System.Timers.Timer _timer1 = new System.Timers.Timer();

    //    private bool _IsStarted = false;
    //    public bool IsStarted
    //    {
    //        get { return _IsStarted; }
    //        set
    //        {
    //            if (_IsStarted != value)
    //            {
    //                _IsStarted = value;
    //            }
    //        }
    //    }
    //    public ConcurrentDictionary<string, T> Items { get; private set; } = new ConcurrentDictionary<string, T>();

    //    private bool _isSimulationMode;  //in simulation mode we does not start timer loop to check order status, alternatviely check order status is triggered externally
    //    private int _timerinterval;

    //    private bool _isProcessPullOrderBusy = false;
    //    private int _pollOrderIntervalPerSymbol = 60;
    //    private ConcurrentDictionary<string, long> _lastUpdateOrderTimePerSymbol = new ConcurrentDictionary<string, long>();
    //    private ConcurrentQueue<string> _queuePullOrder = new ConcurrentQueue<string>();
    //    private ConcurrentQueue<T> _itemsToCheck = new ConcurrentQueue<T>();

    //    //file
    //    private bool _isNeedSaveOrders = false;
    //    private bool _isLoadAndDumpFileBusy = false;
    //    private string _fileNameDump;
    //    private bool _isClearDataWhenInit = false;    //whether to clear data during init
    //    private bool _isProcessCheckQueueBusy;

    //    public List<string> Symbols { get; private set; }


    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<T> OnItemUpdated;
    //    public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<T>> OnItemListUpdated;

    //    public ItemListProxy(Func<string, List<T>> getItem, bool isSimulation, bool isClearDataWhenInit, string dumpFileName = null, int timerInterval = 1000, int updateInterval = 60)
    //    {
    //        _isClearDataWhenInit = isClearDataWhenInit;
    //        _fileNameDump = dumpFileName;
    //        _funcGetItems = getItem;
    //        _isSimulationMode = isSimulation;
    //        _timerinterval = timerInterval;
    //        _pollOrderIntervalPerSymbol = updateInterval;

    //        var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
    //        for (int i = 0; i < Symbols.Count; i++)
    //            _lastUpdateOrderTimePerSymbol.TryAdd(Symbols[i], tnow + 120 + i * _pollOrderIntervalPerSymbol);

    //        if (_isDumpFile)
    //        {
    //            this.OnItemUpdated += (s, t) => this.SetDumpFile(true);
    //            this.OnItemListUpdated += (s, t) => this.SetDumpFile(true);
    //        }
    //    }

    //    void ProcessUpdateQueue()
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
    //                    this.UpdteFromServer(symbol, true);
    //                }
    //            }
    //            finally
    //            {
    //                _isProcessPullOrderBusy = !_isProcessPullOrderBusy;
    //            }
    //        });
    //    }

    //    private void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    //    {
    //        var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();            
    //        foreach (var symbol in Symbols)
    //        {
    //            if (tnow - _lastUpdateOrderTimePerSymbol[symbol] >= _pollOrderIntervalPerSymbol)
    //            {
    //                _lastUpdateOrderTimePerSymbol[symbol] = tnow;
    //                if (!_queuePullOrder.Contains(symbol))
    //                {
    //                    _queuePullOrder.Enqueue(symbol);
    //                    this.ProcessUpdateQueue();
    //                }
    //            }
    //        }

    //        if (_isDumpFile)
    //        {
    //            if (_isNeedSaveOrders)
    //            {
    //                lock (this)
    //                {
    //                    if (_isLoadAndDumpFileBusy) return;
    //                    _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
    //                }

    //                try
    //                {
    //                    this.DumpToFile();
    //                }
    //                catch (Exception ex)
    //                {
    //                    Console.WriteLine(ex.ToString());
    //                }
    //                finally
    //                {
    //                    this.SetDumpFile(false);
    //                    _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
    //                }
    //            }
    //        }
    //    }

    //    private void UpdteFromServer(string symbol, bool isTriggerEvent)
    //    {
    //        var lst = _funcGetItems(symbol);
    //        if (lst != null)
    //        {
    //            foreach (var o in lst)
    //                this.CheckItem(o, isTriggerEvent);
    //        }
    //    }

    //    private void CheckItem(T o, bool isTriggerEvent)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public T GetItemById(string id)
    //    {
    //        if (Items.ContainsKey(id)) return Items[id];
    //        return default(T);
    //    }

    //    public void UpdateItems(params T[] items)
    //    {
    //        if (!this.IsStarted) return;

    //        if (_isSimulationMode)
    //        {
    //            foreach (var t in items) this.CheckItem(t, true);
    //        }
    //        else
    //        {
    //            foreach (var o in items)
    //                _itemsToCheck.Enqueue(o);
    //            this.ProcessCheckQueue();
    //        }
    //    }

    //    void ProcessCheckQueue()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessCheckQueueBusy) return;
    //            _isProcessCheckQueueBusy = true;
    //        }

    //        Task.Run(() =>
    //        {
    //            try
    //            {
    //                T o;
    //                while (_itemsToCheck.TryDequeue(out o))
    //                {
    //                }
    //            }
    //            finally
    //            {
    //                _isProcessCheckQueueBusy = false;
    //            }
    //        });
    //    }

    //    public void Start()
    //    {
    //        if (!_isSimulationMode)
    //        {
    //            if (!IsStarted)
    //            {
    //                this.Initialize();
    //                _timer1.Interval = _timerinterval;
    //                _timer1.Elapsed += _timer1_Elapsed;
    //                _timer1.Start();
    //                IsStarted = true;
    //            }
    //        }
    //        else IsStarted = true;
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

    //    void Initialize()
    //    {
    //        lock (this)
    //        {
    //            if (_isProcessItemBusy) return;
    //            _isProcessItemBusy = true;
    //        }

    //        try
    //        {
    //            if (_isDumpFile)
    //                this.LoadFromFile();
    //            else if (_isClearDataWhenInit)
    //                this.Items.Clear();

    //            var lst = _funcGetItems();
    //            if (lst != null)
    //            {
    //                foreach (var t in lst)
    //                    this.Items.TryAdd(t.Id, t);
    //                OnItemListUpdated?.Invoke(this, this.Items.Values.ToList());
    //            }
    //        }
    //        finally
    //        {
    //            _isProcessItemBusy = false;
    //        }
    //    }

    //    void SetDumpFile(bool v)
    //    {
    //        lock (this)
    //        {
    //            _isNeedSaveOrders = v;
    //        }
    //    }

    //    void DumpToFile()
    //    {
    //        if (_isDumpFile && !string.IsNullOrEmpty(_fileNameDump))
    //        {
    //            var str = new StringBuilder();
    //            var datestr = DateTime.UtcNow.ToString("yyyyMMdd");
    //            var datapath = Path.GetDirectoryName(_fileNameDump);
    //            str.Append(JsonConvert.SerializeObject(Items, Formatting.Indented));
    //            File.WriteAllText(Path.Combine(datapath, $"{datestr}_{Path.GetFileName(_fileNameDump)}"), str.ToString());
    //        }
    //    }

    //    void LoadFromFile()
    //    {
    //        if (_isDumpFile)
    //        {
    //            try
    //            {
    //                if (File.Exists(this._fileNameDump))
    //                {
    //                    var str = File.ReadAllText(this._fileNameDump);
    //                    this.Items = JsonConvert.DeserializeObject<ConcurrentDictionary<string, T>>(str);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(ex.ToString());
    //            }
    //        }
    //    }
    //}
}
