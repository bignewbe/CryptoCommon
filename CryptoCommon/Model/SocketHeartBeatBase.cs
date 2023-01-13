using CryptoCommon.Interface;
using CryptoCommon.Shared;
using PortableCSharpLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommon.Model
{
    public class SocketHeartBeatBase : SocketBase, ISocketHeartbeatBase
    {
        //string url = "wss://real.okex.com:10442/ws/v3";
        //string _url = "wss://stream.binance.com:9443/ws";
        //string publicUrl = "wss://ws.okex.com:8443/ws/v5/public";
        //string privateUrl = "wss://ws.okex.com:8443/ws/v5/private";

        //private bool _isCheckTimerBusy;
        //private System.Timers.Timer _checkTimer = new System.Timers.Timer();
        private bool _isPingSent;
        private long _lastPingTime;
        //private long _lastPongTime;

        //public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<long> OnHeartbeatStopped;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<long> OnHeartbeatReceived;

        Func<string, bool> _funcCheckPong;
        string _pingMsg;
        bool _isCheckHeartBeat;
        int _heartBeatIntervalMili;

        public SocketHeartBeatBase(string url, int receiveOvertimeSeconds,
            int heartBeatIntervalMili, string pingMsg, Func<string, bool> funcCheckPong) : base(url, receiveOvertimeSeconds)
        //int heartBeatIntervalMili = 5000, string pingMsg = "ping", Func<string,bool> funcCheckPong) : base(url, receiveOvertimeSeconds)
        {
            _pingMsg = pingMsg;
            _funcCheckPong = funcCheckPong;
            _heartBeatIntervalMili = heartBeatIntervalMili;
            _isCheckHeartBeat = _heartBeatIntervalMili > 0 && !string.IsNullOrEmpty(_pingMsg) && _funcCheckPong != null;

            if (_isCheckHeartBeat)
            {
                //_checkTimer.Interval = 100;
                //_checkTimer.Elapsed += _checkTimer_Elapsed;
                this.OnMsgReceived += _OnMsgReceived;
                //this.OnConnectionChanged += _socket_OnConnectionChanged;
                //this.OnReceiveOverTime += (s, e) => this.SendAsync("ping").Wait();
            }
        }

        private void _OnMsgReceived(object sender, string item)
        {
            if (_isCheckHeartBeat && _isPingSent)
            {
                if (_funcCheckPong(item))
                {
                    Console.WriteLine("pong message received");
                    //_lastPongTime = DateTime.UtcNow.GetMiliSecondsFromUTC();
                    _isPingSent = false;
                    OnHeartbeatReceived?.Invoke(this, DateTime.UtcNow.GetMiliSecondsFromUTC());
                }
            }
        }

        protected override async Task TimerFunction()
        {
            await base.TimerFunction();

            var tnow = DateTime.UtcNow.GetMiliSecondsFromUTC();
            if (tnow - _lastPingTime >= _heartBeatIntervalMili)  //send ping every 20 seconds
            {
                this.SendAsync(_pingMsg).Wait();
                Console.WriteLine("socket ping message sent");
                _lastPingTime = tnow;
                _isPingSent = true;
            }

            ////we expect to receive pong every 20 seconds
            //if (tnow - _lastPongTime >= _heartBeatIntervalMili + 500)    
            //{
            //    Console.WriteLine("Heartbeat stopped!!!");
            //    OnHeartbeatStopped?.Invoke(this, tnow - _lastPongTime);
            //}
        }

        //private void _socket_OnConnectionChanged(object sender, WebSocketState item)
        //{
        //    if (item == WebSocketState.Open)
        //    {
        //        this.Login().Wait();
        //        this.OnLoginCompleted?.Invoke(this, true);
        //    }
        //}

        //private void _checkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    lock (this)
        //    {
        //        if (_isCheckTimerBusy) return;
        //        _isCheckTimerBusy = !_isCheckTimerBusy;
        //    }

        //    try
        //    {
        //        var tnow = DateTime.UtcNow.GetMiliSecondsFromUTC();
        //        if (tnow - _lastPingTime >= _heartBeatIntervalMili)  //send ping every 20 seconds
        //        {
        //            this.SendAsync(_pingMsg).Wait();
        //            Console.WriteLine("socket ping message sent");
        //            _lastPingTime = tnow;
        //            _isPingSent = true;
        //            Thread.Sleep(1000);  //make sure pong is received
        //        }

        //        if (tnow - _lastPongTime >= _heartBeatIntervalMili + 500)    //we expect to receive pong every 20 seconds
        //        {
        //            Console.WriteLine("Heartbeat stopped!!!");
        //            OnHeartbeatStopped?.Invoke(this, tnow - _lastPongTime);
        //        }
        //    }
        //    finally
        //    {
        //        _isCheckTimerBusy = !_isCheckTimerBusy;
        //    }
        //}

    }
}
