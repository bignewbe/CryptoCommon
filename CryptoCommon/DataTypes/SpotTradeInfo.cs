using PortableCSharpLib;
using PortableCSharpLib.Interace;
using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCommon.DataTypes
{
    public class LocalProxyStatus
    {
        Func<long> _funcGetCurrentTime;
        long _timestamp;
        HashSet<string> _refIdSubmit = new HashSet<string>();
        HashSet<string> _refIdCancel = new HashSet<string>();

        public LocalProxyStatus(Func<long> funcGetCurrentTime = null)
        {
            _funcGetCurrentTime = funcGetCurrentTime;
        }

        private void SetTimestamp()
        {
            _timestamp = _funcGetCurrentTime == null ? DateTime.UtcNow.GetUnixTimeFromUTC() : _funcGetCurrentTime();
        }
        public void IncNumOrderSubmit(string refId)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(refId)) return;
                if (!_refIdSubmit.Contains(refId)) _refIdSubmit.Add(refId);
                this.SetTimestamp();
            }
        }
        public void DecNumOrderSubmit(string refId)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(refId)) return;
                if (_refIdSubmit.Contains(refId)) _refIdSubmit.Remove(refId);
                this.SetTimestamp();
            }
        }
        public void IncNumOrderCancel(string refId)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(refId)) return;
                if (!_refIdCancel.Contains(refId)) _refIdCancel.Add(refId);
                this.SetTimestamp();
            }
        }
        public void DecNumOrderCancel(string refId)
        {
            lock (this)
            {
                if (string.IsNullOrEmpty(refId)) return;
                if (_refIdCancel.Contains(refId)) _refIdCancel.Remove(refId);
                this.SetTimestamp();
            }
        }
        public bool IsHandlingOrderFinished(long tnow)
        {
            lock (this)
            {
                if (tnow - _timestamp > 600)
                {
                    _refIdCancel.Clear();
                    _refIdSubmit.Clear();
                }
                return (_refIdSubmit.Count == 0 && _refIdCancel.Count == 0);
            }
        }
    }

    public class SpotTradeInfo : LocalProxyStatus, IIdEqualCopy<SpotTradeInfo>
    {
        public double? _prevSoldPrice;
        public double? _prevBoughtPrice;
        public long _lastRefreshLocalStatusTime;
        public SpotOrder _lastOrder;
        public bool _isInitialized { get { return _lastRefreshLocalStatusTime > 0; } }
        public long? _prevCreatedTimeSold { get; set; }
        public long? _prevCreatedTimeBought { get; set; }

        public string Id { get { return ParamId; } }
        public string ParamId { get; set; }
        public string Symbol { get; private set; }
        public string C1 { get; private set; }
        public string C2 { get; private set; }

        public double QtyBuyOpen { get { return ExptC1; } }
        public double QtySellOpen { get { return HoldC1; } }

        public double NetC1 { get; set; }
        public double NetC2 { get; set; }
        public double HoldC1 { get; set; }
        public double ExptC2 { get; set; }
        public double HoldC2 { get; set; }
        public double ExptC1 { get; set; }
        public double AvailC1 { get; set; }
        public double AvailC2 { get; set; }
        public double Ratio { get; set; }

        public int NumOpenBuyOrder { get; set; }
        public int NumOpenSellOrder { get; set; }
        public int NumBoughtOrder { get; set; }
        public int NumSoldOrder { get; set; }
        public double QtySold { get; set; }
        public double QtyBought { get; set; }
        public double AvgSoldPrice { get; set; }
        public double AvgBoughtPrice { get; set; }

        public SpotTradeInfo()
        {
        }

        public SpotTradeInfo(string paramId, string symbol, Func<long> funcGetCurrentTime = null) : base(funcGetCurrentTime)
        {
            this.ParamId = paramId;
            this.Symbol = symbol;
            if (!this.Symbol.Contains("CON"))
            {
                C1 = Symbol.Split('_')[0];
                C2 = Symbol.Split('_')[1];
            }
        }

        public void Copy(SpotTradeInfo other)
        {
            if (other == null) return;
            this.ParamId = other.ParamId;
            this.Symbol = other.Symbol;
            this.C1 = other.C1;
            this.C2 = other.C2;

            this.NetC1 = other.NetC1;
            this.NetC1 = other.NetC1;
            this.NetC2 = other.NetC2;
            this.HoldC1 = other.HoldC1;
            this.ExptC2 = other.ExptC2;
            this.HoldC2 = other.HoldC2;
            this.ExptC1 = other.ExptC1;
            this.AvailC1 = other.AvailC1;
            this.AvailC2 = other.AvailC2;
            this.Ratio = other.Ratio;
            this.NumOpenBuyOrder = other.NumOpenBuyOrder;
            this.NumOpenSellOrder = other.NumOpenSellOrder;
            this.NumBoughtOrder = other.NumBoughtOrder;
            this.NumSoldOrder = other.NumSoldOrder;
            this.QtySold = other.QtySold;
            this.QtyBought = other.QtyBought;
            this.AvgSoldPrice = other.AvgSoldPrice;
            this.AvgBoughtPrice = other.AvgBoughtPrice;
            this._prevCreatedTimeSold = other._prevCreatedTimeSold;
            this._prevCreatedTimeBought = other._prevCreatedTimeBought;
        }
        public bool Equals(SpotTradeInfo other)
        {
            if (other == null) return false;

            return (
            this.ParamId == other.ParamId &&
            this.NetC1 == other.NetC1 &&
            this.NetC1 == other.NetC1 &&
            this.NetC2 == other.NetC2 &&
            this.HoldC1 == other.HoldC1 &&
            this.ExptC2 == other.ExptC2 &&
            this.HoldC2 == other.HoldC2 &&
            this.ExptC1 == other.ExptC1 &&
            this.AvailC1 == other.AvailC1 &&
            this.AvailC2 == other.AvailC2 &&
            this.Ratio == other.Ratio &&
            this.NumOpenBuyOrder == other.NumOpenBuyOrder &&
            this.NumOpenSellOrder == other.NumOpenSellOrder &&
            this.NumBoughtOrder == other.NumBoughtOrder &&
            this.NumSoldOrder == other.NumSoldOrder &&
            this.QtySold == other.QtySold &&
            this.QtyBought == other.QtyBought &&
            this.AvgSoldPrice == other.AvgSoldPrice &&
            this.AvgBoughtPrice == other.AvgBoughtPrice &&
            this._prevCreatedTimeSold == other._prevCreatedTimeSold &&
            this._prevCreatedTimeBought == other._prevCreatedTimeBought);
        }
    }
}
