using CryptoCommon.Interfaces;
using CryptoCommon.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
            _exchange = _config["Exchange"];

            //create meta data
            var config1 = _appConfig.GetSection("Path");
            _logPath = config1["logPath"];
            _dataPath = config1["dataPath"];
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

    }
}
