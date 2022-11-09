using PortableCSharpLib;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoCommon.Shared
{
    public class SocketBase : IDisposable
    {
        string _url;
        int _receiveOvertimeSeconds;
        ClientWebSocket _ws = null;
        CancellationTokenSource _cts = new CancellationTokenSource();

        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<WebSocketState> OnConnectionChanged;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<int> OnReceiveOverTime;
        public event PortableCSharpLib.EventHandlers.ItemChangedEventHandler<string> OnMsgReceived;
        private ConcurrentQueue<string> _pendingChannels = new ConcurrentQueue<string>();  //save channel for unknow socket state

        //timer for periodically check the status and reboot
        private System.Timers.Timer _closeCheckTimer = new System.Timers.Timer();
        private bool _isTimerBusy;
        private long _lastReceiveTime;
        private bool _isSendAsyncBusy;

        public bool IsStartRequested { get; private set; }  //has user pressed "start"
        public WebSocketState State { get; private set; } = WebSocketState.None;

        public SocketBase(string url, int receiveOvertimeSeconds = 10)
        {
            _url = url;
            _receiveOvertimeSeconds = receiveOvertimeSeconds;

            _ws = new ClientWebSocket();
            _closeCheckTimer.Interval = 100;
            _closeCheckTimer.Elapsed += CloseCheckTimer_Elapsed;
        }

        public async Task StartAsync()
        {
            this.IsStartRequested = true;

            if (_ws.State != WebSocketState.Open)
            {
                await _ws.ConnectAsync(new Uri(_url), _cts.Token);
                while (_ws.State != WebSocketState.Open) 
                    Thread.Sleep(50);
                this.receive();                                       //start a new thread to receive data
            }

            if (!_closeCheckTimer.Enabled)
                _closeCheckTimer.Start();

            //if (!this.IsConnected)
            //{
            //    await _ws.ConnectAsync(new Uri(_url), _cts.Token);
            //    this.receive();          //start a new thread to receive data
            //    _closeCheckTimer.Start();
            //}
        }

        public async Task StopAsync()
        {
            this.IsStartRequested = false;
            if (_closeCheckTimer.Enabled)
                _closeCheckTimer.Stop();

            if (_ws.State == WebSocketState.Open)
            {
                _closeCheckTimer.Stop();
                await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, _cts.Token);
                _cts.Cancel();
            }
        }

        private void receive()
        {
            Task.Factory.StartNew(
              async () =>
              {
                  while (!_cts.Token.IsCancellationRequested)
                  {
                      if (_ws.State == WebSocketState.Open)
                      {
                          byte[] buffer = new byte[200000];
                          var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                          if (result.MessageType == WebSocketMessageType.Binary)
                          {
                              _lastReceiveTime = DateTime.UtcNow.GetUnixTimeFromUTC();
                              var resultStr = Decompress(buffer);
                              OnMsgReceived?.Invoke(this, resultStr);
                          }
                          else if (result.MessageType == WebSocketMessageType.Text)
                          {
                              _lastReceiveTime = DateTime.UtcNow.GetUnixTimeFromUTC();
                              var resultStr = System.Text.Encoding.UTF8.GetString(buffer);
                              OnMsgReceived?.Invoke(this, resultStr);
                          }
                          else if (result.MessageType == WebSocketMessageType.Close)
                          {
                              try
                              {
                                  await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, _cts.Token);
                              }
                              catch (Exception)
                              {
                                  break;
                              }
                              break;
                          }
                      }
                  }
              }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private string Decompress(byte[] baseBytes)
        {
            using (var decompressedStream = new MemoryStream())
            using (var compressedStream = new MemoryStream(baseBytes))
            using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(decompressedStream);
                decompressedStream.Position = 0;
                using (var streamReader = new StreamReader(decompressedStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 1. release all resources
        /// 2. connect and login
        /// 3. subscribe saved channel
        /// 4. subscribe pending channel
        /// </summary>
        /// <returns></returns>
        /// 
        private async void CloseCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (this)
            {
                if (_isTimerBusy) return;
                _isTimerBusy = !_isTimerBusy;
            }

            try
            {
                if (_ws != null && _ws.State != this.State)
                {
                    this.State = _ws.State;
                    OnConnectionChanged?.Invoke(this, State);
                }

                if (this.IsStartRequested && _ws.State != WebSocketState.Open)               //reconnect
                {     
                    await Reconnect();
                }
                else if (this.IsStartRequested && _ws.State == WebSocketState.Open)          //send pending channel
                {
                    var tnow = DateTime.UtcNow.GetUnixTimeFromUTC();
                    if (tnow - _lastReceiveTime > _receiveOvertimeSeconds)
                    {
                        _lastReceiveTime = tnow;
                        OnReceiveOverTime?.Invoke(this, (int)(tnow - _lastReceiveTime));
                    }

                    if (_pendingChannels.Count > 0)
                    {
                        string channel;
                        while (_pendingChannels.TryDequeue(out channel))
                        {
                            await this.SendAsync(channel);
                        }
                    }
                }
                else if (!this.IsStartRequested && _ws.State == WebSocketState.Open)         //stop
                {
                    await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, _cts.Token);
                    _cts.Cancel();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("reconnecting after 10 seconds...");
                Thread.Sleep(10000);
            }
            finally
            {
                _isTimerBusy = !_isTimerBusy;
            }
        }

        public async Task Reconnect()
        {
            try
            {
                Console.WriteLine($"\n trying to close socket with State = {_ws.State}");
                await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, _cts.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("error while closing socket. sleep for 10 seconds....");
                Thread.Sleep(10000);
            }

            Console.WriteLine($"socket state = {_ws.State} trying to reconnect ...");

            //cancel the receive thread loop
            if (_cts.Token.CanBeCanceled)
            {
                _cts.Cancel();
                _cts = new CancellationTokenSource();
            }

            _closeCheckTimer.Stop();
            _ws.Abort();
            _ws.Dispose();
            _ws = null;
            _ws = new ClientWebSocket();
            this.State = _ws.State;

            await _ws.ConnectAsync(new Uri(_url), _cts.Token);
            while (_ws.State != WebSocketState.Open)
                Thread.Sleep(50);

            Thread.Sleep(1000);
            this.receive();                                                          
            _closeCheckTimer.Start();

            Thread.Sleep(10000);
            Console.WriteLine($"socket state = {_ws.State}");
        }

        //private async Task CloseSocket()
        //{
        //    if (_ws.State != WebSocketState.Aborted && _ws.State != WebSocketState.Closed)
        //    {
        //        try
        //        {
        //            Console.WriteLine($"\n trying to close socket with State = {_ws.State}");
        //            await _ws.CloseOutputAsync(WebSocketCloseStatus.Empty, null, _cts.Token);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex);
        //            Console.WriteLine("error while closing socket. sleep for 10 seconds....");
        //            Thread.Sleep(10000);
        //        }
        //    }
        //}

        protected async Task SendAsync(string msg)
        {
            lock (this)
            {
                if (_isSendAsyncBusy) return;
                _isSendAsyncBusy = !_isSendAsyncBusy;
            }

            try
            {
                if (_ws.State == WebSocketState.Open)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes(msg);
                    await _ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    _pendingChannels.Enqueue(msg);
                }
            }
            finally
            {
                _isSendAsyncBusy = !_isSendAsyncBusy;
            }
        }

        public void Dispose()
        {
            if (!_cts.Token.CanBeCanceled)
            {
                _cts.Cancel();
            }
            if (_ws != null)
            {
                _ws.Dispose();
                _ws = null;
            }
            if (_closeCheckTimer != null)
            {
                _closeCheckTimer.Stop();
                _closeCheckTimer.Dispose();
            }
        }
    }
}
