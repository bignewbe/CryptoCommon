using CryptoCommon.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommonUnitTest
{
    //apikey = "e869b48a-5058-42a1-9587-9029d96bd6f9"
    //secretkey = "8E7D2E67C8296ED12D2BD44CC70476A8"
    //IP = ""
    //备注名 = "key1"
    //权限 = "读取/交易"
    //WebSocket公共频道：wss://wspap.okx.com:8443/ws/v5/public?brokerId=9999
    //WebSocket私有频道：wss://wspap.okx.com:8443/ws/v5/private?brokerId=9999
    [TestClass]
    public class TestSocketBase
    {
        const string _pubUrl = "wss://wspap.okx.com:8443/ws/v5/public?brokerId=9999";
        const string _prvUrl = "wss://wspap.okx.com:8443/ws/v5/public?brokerId=9999";
        public TestSocketBase()
        {
        }

        [TestMethod]
        public async Task TestPingPong()
        {
            var socket = new SocketBase(_pubUrl, 3);
            var msg = string.Empty;
            var isConnected = false;
            var state = WebSocketState.None;
            var isReceiveOverTime = false;
            socket.OnReceiveOverTime += (s, e) => isReceiveOverTime = true;
            socket.OnConnectionChanged += (s, e) => state = e;
            socket.OnMsgReceived += (s, m) =>
            {
                msg = m;
                isReceiveOverTime = false;
            };
            await socket.StartAsync();
            Assert.IsTrue(state == WebSocketState.Open);
            await socket.StopAsync();
            Assert.IsTrue(state != WebSocketState.Open);
            await socket.StartAsync();
            Assert.IsTrue(state == WebSocketState.Open);
            await socket.Reconnect();
            Assert.IsTrue(state == WebSocketState.Open);

            msg = "";
            await socket.SendAsync("ping");
            await Task.Delay(1000);
            Assert.IsTrue(msg.Contains("pong"));

            isReceiveOverTime = false;
            await Task.Delay(4000);
            Assert.IsTrue(isReceiveOverTime);
            await socket.StopAsync();
            Assert.IsTrue(state != WebSocketState.Open);

            //reconnect wont do anything if ws is stopped
            await socket.Reconnect();
            Assert.IsTrue(state != WebSocketState.Open);
        }

        [TestMethod]
        public async Task TestReceiveOverTime()
        {
            var socket = new SocketHeartBeatBase(_pubUrl, 3, 5000, "ping", (msg) => msg.Contains("pong"));
            var overTimeCount = 0;
            var heartBeatCount = 0;

            socket.OnReceiveOverTime += (s, e) => ++overTimeCount;
            socket.OnHeartbeatReceived += (s, m) => ++heartBeatCount;

            await socket.StartAsync();
            await Task.Delay(4000);
            Assert.IsTrue(heartBeatCount == 1);  //first heartbeat sent when start
            Assert.IsTrue(overTimeCount == 1);   //no data received more than 4 seconds
            await Task.Delay(5000);
            Assert.IsTrue(heartBeatCount == 2);   // second hearbeat sent at 5 
            Assert.IsTrue(overTimeCount == 2);    // no data received more than 3.5 seconds
        }


        [TestMethod]
        public async Task TestHeartBeat()
        {
            var socket = new SocketHeartBeatBase(_pubUrl, 4, 2000, "ping", (msg)=>msg.Contains("pong"));
            var overTimeCount = 0;
            var heartBeatCount = 0;

            socket.OnReceiveOverTime += (s, e) => ++overTimeCount;
            socket.OnHeartbeatReceived += (s, m) => ++heartBeatCount;

            await socket.StartAsync();
            await Task.Delay(4000);
            Assert.IsTrue(overTimeCount == 0);
            Assert.IsTrue(heartBeatCount > 0);
        }
    }
}
