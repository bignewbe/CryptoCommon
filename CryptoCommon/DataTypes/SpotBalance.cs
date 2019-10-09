using PortableCSharpLib.Interace;

namespace CryptoCommon.DataTypes
{
    public class SpotBalance : IIdEqualCopy<SpotBalance>
    {
        public string Id { get { return Currency; } }
        public double Balance { get { return Available + Hold; } }

        public string Currency { get; set; }
        public double Available { get; set; }
        public double Hold { get; set; }

        public SpotBalance()
        {
        }

        public SpotBalance(SpotBalance other)
        {
            this.Copy(other);
        }

        public void Copy(SpotBalance other)
        {
            if (other != null)
            {
                this.Currency = other.Currency;
                this.Available = other.Available;
                this.Hold = other.Hold;
            }
        }

        public bool Equals(SpotBalance other)
        {
            if (other == null) return false;

            return (this.Currency == other.Currency &&
                    this.Available == other.Available &&
                    this.Hold == other.Hold);
        }
    }
}
