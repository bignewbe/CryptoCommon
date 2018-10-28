using System;

namespace CryptoCommon.DataTypes
{
    public class OHLC : IEquatable<OHLC>
    {
        public string Symbol { get; set; }
        public int Interval { get; set; }
        public long Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public OHLC() { }
        public OHLC(OHLC other) => this.Copy(other);
        public void Copy(OHLC other)
        {
            this.Symbol = other.Symbol;
            this.Interval = other.Interval;
            this.Time = other.Time;
            this.Open = other.Open;
            this.High = other.High;
            this.Low = other.Low;
            this.Close = other.Close;
            this.Volume = other.Volume;
        }

        public bool Equals(OHLC other)
        {
            return
                  this.Symbol == other.Symbol &
                  this.Interval == other.Interval &
                  this.Time == other.Time &
                  this.Open == other.Open &
                  this.High == other.High &
                  this.Low == other.Low &
                  this.Close == other.Close &
                  this.Volume == other.Volume;
        }
    }
}
