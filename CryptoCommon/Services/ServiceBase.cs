using CommonCSharpLibary.Model;
using CryptoCommon.Interfaces;
using CryptoCommon.Models;
using Microsoft.Extensions.Configuration;
using PortableCSharpLib.TechnicalAnalysis;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;

namespace CryptoCommon.Services
{
    public class ServiceBase
    {
        protected static ManualResetEvent _quitEvent = new ManualResetEvent(false);
        protected static string _exchange;
        protected static IProductMeta _meta;
        protected static string _env;
        protected static IConfigurationRoot _appConfig;
        protected static IConfigurationSection _config;
        protected static string _servicename;
        protected static string _logPath;
        protected static string _dataPath;

        protected static void Init(string servcieName)
        {
            Console.Title = _servicename = servcieName;

            _env = Environment.GetEnvironmentVariable("ENV");
            _appConfig = LoadAppSettings.LoadSettings();
            if (_appConfig == null)
            {
                Console.WriteLine("appsettins.json not found!!!");
                return;
            }

            _config = _appConfig.GetSection(_servicename);
            if (_config == null)
            {
                Console.WriteLine($"{_servicename} does not exist in appsettings.json");
                return;
            }

            ////////////////////////////////////////////////////////
            _exchange = _config["Exchange"];
            var config1 = _appConfig.GetSection("Path");
            _dataPath = Path.Combine(config1["dataPath"], _exchange);
            _logPath = Path.Combine(_dataPath, "logs");
            
            var metaPath = config1["metaPath"];
            _meta = new ProductMeta(metaPath);
            Console.WriteLine($"metaPath = {metaPath}");
            Console.WriteLine("waiting to connect to service keeper....");
            if (_env != null && _env != "DEV")
            {
                var waittime = int.Parse(_config["WaitTime"]);
                Thread.Sleep(waittime);                     //wait a bit longer since this service dependends of capture service
            }
            //Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(appConfig).CreateLogger();
            //ZookeeperFactory.ConfigSerilogDefault(config1["logPath"], _servicename);
        }

        //protected void StartQuoteService(ISpotMarket marketSecond)
        //{
        //    string hostIp;
        //    var provider = ZookeeperFactory.CreateProviderDefault(_config, out hostIp);
        //    var messageClient = ZookeeperFactory.CreateMessageClientDefaul(_config);
        //    var serviceConsumer = ZookeeperFactory.CreateConsumerDefault(_config);

        //    ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    Policy.Handle<Exception>()
        //          .WaitAndRetry(5, r => TimeSpan.FromSeconds(5), (ex, ts) => { Log.Information("Error connecting to service keeper. Retrying in 5 sec."); })
        //          .Execute(() => provider.StartAsync().Wait());
        //    Policy.Handle<Exception>()
        //          .WaitAndRetry(5, r => TimeSpan.FromSeconds(5), (ex, ts) => { Log.Information("Error connecting to message keeper. Retrying in 5 sec."); })
        //          .Execute(() => messageClient.StartAsync().Wait());
        //    Log.Information("connected");

        //    /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    //register service
        //    //var marketSecond = new SpotMarketHuobi(_meta);
        //    var captureService = new SpotCaptureService(marketSecond);

        //    var numTickers = int.Parse(_config["NumTickers"]);
        //    var numBars = int.Parse(_config["NumBars"]);
        //    var dataPath = _dataPath;
        //    var tickStore = new TickerStore();
        //    var fileStore = new QuoteBasicFileStore(_exchange, dataPath, 100000);
        //    var qcStore = new QuoteCaptureMemStore(_exchange, numTickers);
        //    var intervals = _config.GetSection("Intervals").Get<string[]>().Select(s => int.Parse(s)).ToList();
        //    var qbStore = new QuoteBasicMemStore(_exchange, numBars, intervals);

        //    var quoteService = new CryptoCommon.Services.QuoteService(captureService, tickStore, qcStore, qbStore, fileStore);
        //    provider.RegisterServices($"{_exchange}QuoteService", hostIp, quoteService);
        //    Log.Information($"{_exchange}QuoteService registered with ip = {hostIp} and port = {provider.Port}");

        //    /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //    ///register message
        //    quoteService.OnExceptionOccured += (object sender, string exchange, Exception ex) =>
        //    {
        //        Log.Error($"{exchange}: {ex.ToString()}");
        //    };

        //    quoteService.OnQuoteBasicDataAddedOrUpated += (object sender, string exch, IQuoteBasicBase quote, int numAppended) =>
        //    {
        //        if (quote.Interval == 60)
        //        {
        //            var j = numAppended == 0 ? quote.Count - 1 : quote.Count - numAppended;
        //            var lst = new List<OHLC>();
        //            for (int i = j; i < quote.Count; i++)
        //            {
        //                var ohlc = new OHLC
        //                {
        //                    Symbol = quote.Symbol,
        //                    Interval = quote.Interval,
        //                    Time = quote.Time[i],
        //                    Open = quote.Open[i],
        //                    Close = quote.Close[i],
        //                    High = quote.High[i],
        //                    Low = quote.Low[i],
        //                    Volume = quote.Volume[i]
        //                };
        //                lst.Add(ohlc);
        //            }
        //            //if (quote.Symbol == "XUC_USDT")
        //            var messageId = $"{_exchange}_SpotCandleList"; // MessageId.Okex_Spot_QuoteBasicDataAppended.ToString();
        //            //Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")} broadcast {messageId} {quote.QuoteID} at {lst[0].Time} {lst[0].Time.GetUTCFromMiliSeconds()}");
        //            messageClient.RequestBroadcast(messageId, lst);
        //        }
        //    };

        //    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////                
        //    messageClient.OnReceiveBroadcast += (object sender, string id, string data) =>
        //    {
        //        if (id == $"{_exchange}SpotCandleList")
        //        {
        //            _timestamp = DateTime.UtcNow.GetUnixTimeFromUTC();
        //            var candles = JsonConvert.DeserializeObject<List<OHLC>>(data);
        //            quoteService.AddCandleList(candles);
        //        }
        //    };

        //    //messageClient.SubscribeBroadcast($"{_exchange}SpotCandleList");
        //    _timer.Elapsed += (object sender, ElapsedEventArgs e) =>
        //    {
        //        var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
        //        if (tnow - _timestamp > 60)   // we dont receive data from server for more than 60 seconds
        //        {
        //            messageClient.SubscribeBroadcast($"{_exchange}SpotCandleList");
        //        }
        //    };
        //    _timer.Interval = 1000;
        //    _timer.Start();
        //}
    }
}
