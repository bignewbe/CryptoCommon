using Newtonsoft.Json;
using PortableCSharpLib;
using PortableCSharpLib.Interace;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Shared.ExchProxy
{
    public interface IGenericProxy<T>
    {
        ConcurrentDictionary<string, T> Items { get; }
        T GetItemById(string id);

        void Update(params T[] items);

        bool IsStarted { get; }
        void Start();
        void Stop();

        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<T> OnItemUpdated;
        event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<T>> OnItemListUpdated;
    }

    public class GenericProxy<T>: IGenericProxy<T> where T : class, IIdEqualCopy<T>
    {
        public static GenericProxy<T> CreateProxyWithoutPoll()
        {
            return new GenericProxy<T>(getItem: null, timerInterval: -1, updateInterval: -1);
        }

        Func<List<T>> _funcGetItems;

        private bool _isDumpFile => !string.IsNullOrEmpty(_fileNameDump);
        private bool _isProcessItemBusy = false;
        private System.Timers.Timer _timer1 = new System.Timers.Timer();

        private bool _IsStarted = false;
        public bool IsStarted
        {
            get { return _IsStarted; }
            set
            {
                if (_IsStarted != value)
                {
                    _IsStarted = value;
                }
            }
        }
        public ConcurrentDictionary<string, T> Items { get; private set; } = new ConcurrentDictionary<string, T>();

        private double _timerinterval;     //timmer interval
        private long _lastUpdateTime;
        private int _updateInterval;       //interval to update data from server

        //file
        private bool _isNeedSaveOrders = false;
        private bool _isLoadAndDumpFileBusy = false;
        private string _fileNameDump;

        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<T> OnItemUpdated;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<List<T>> OnItemListUpdated;

        public GenericProxy(Func<List<T>> getItem=null, string dumpFileName = null, int timerInterval = -1, int updateInterval = -1)
        {
            if (!string.IsNullOrEmpty(dumpFileName) && timerInterval < 0)
                throw new Exception($"timer interval cannot be 0 when we need to dump file from time to time");

            _fileNameDump = dumpFileName;
            _funcGetItems = getItem;
            _timerinterval = timerInterval;
            _updateInterval = updateInterval;
            _lastUpdateTime = DateTime.UtcNow.GetUnixTimeFromUTC();

            if (_isDumpFile)
            {
                this.OnItemUpdated += (s, t) => this.SetDumpFile(true);
                this.OnItemListUpdated += (s, t) => this.SetDumpFile(true);
            }
        }

        private void _timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_updateInterval > 0)
            {
                var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
                if (tnow - _lastUpdateTime > _updateInterval)
                {
                    _lastUpdateTime = tnow;
                    this.UpdateFromServer();
                }
            }

            if (_isDumpFile)
            {
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
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        this.SetDumpFile(false);
                        _isLoadAndDumpFileBusy = !_isLoadAndDumpFileBusy;
                    }
                }
            }
        }

        private void UpdateFromServer()
        {
            lock (this)
            {
                if (_isProcessItemBusy) return;
                _isProcessItemBusy = !_isProcessItemBusy;
            }

            Task.Run(() =>
            {
                try
                {
                    //this.LogDebug("pull balance");
                    if (_funcGetItems != null)
                    {
                        var lst = _funcGetItems();
                        if (lst != null)
                        {
                            Items.Clear();
                            foreach (var t in lst)
                                this.Items.TryAdd(t.Id, t);
                            OnItemListUpdated?.Invoke(this, this.Items.Values.ToList());
                        }
                    }
                }
                finally
                {
                    _isProcessItemBusy = !_isProcessItemBusy;
                }
            });
        }

        public T GetItemById(string id)
        {
            if (Items.ContainsKey(id)) return Items[id];
            return default(T);
        }

        public void Update(params T[] items)
        {
            foreach (var t in items)
            {
                if (!Items.ContainsKey(t.Id)) Items.TryAdd(t.Id, default(T));
                if (!Items[t.Id].Equals(t))
                {
                    Items[t.Id].Copy(t);
                    OnItemUpdated?.Invoke(this, t);
                }
            }
        }

        public void Start()
        {
            if (!IsStarted)
            {
                this.Initialize();
                if (_timerinterval > 0)
                {
                    _timer1.Interval = _timerinterval;
                    _timer1.Elapsed += _timer1_Elapsed;
                    _timer1.Start();
                }
                IsStarted = true;
            }
        }

        public void Stop()
        {
            if (IsStarted)
            {
                if (_timerinterval > 0)
                {
                    _timer1.Elapsed -= _timer1_Elapsed;
                    _timer1.Stop();
                }
                IsStarted = false;
            }
        }

        void Initialize()
        {
            if (_isDumpFile)
                this.LoadFromFile();

            if (_funcGetItems != null)
            {
                var lst = _funcGetItems();
                if (lst != null)
                {
                    foreach (var t in lst)
                        this.Items.TryAdd(t.Id, t);
                    OnItemListUpdated?.Invoke(this, this.Items.Values.ToList());
                }
            }
        }

        void SetDumpFile(bool v)
        {
            lock (this)
            {
                _isNeedSaveOrders = v;
            }
        }

        void DumpToFile()
        {
            if (_isDumpFile && !string.IsNullOrEmpty(_fileNameDump))
            {
                var str = new StringBuilder();
                var datestr = DateTime.UtcNow.ToString("yyyyMMdd");
                var datapath = Path.GetDirectoryName(_fileNameDump);
                str.Append(JsonConvert.SerializeObject(Items, Formatting.Indented));
                File.WriteAllText(Path.Combine(datapath, $"{datestr}_{Path.GetFileName(_fileNameDump)}"), str.ToString());
            }
        }

        void LoadFromFile()
        {
            if (_isDumpFile)
            {
                try
                {
                    if (File.Exists(this._fileNameDump))
                    {
                        var str = File.ReadAllText(this._fileNameDump);
                        this.Items = JsonConvert.DeserializeObject<ConcurrentDictionary<string, T>>(str);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
