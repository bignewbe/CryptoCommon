using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataTypes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CryptoCommon.DataBase
{
    public class OrderDbContext : DbContext
    {
        public DbSet<FZOrderEnt> Orders { get; set; }

        public OrderDbContext(DbContextOptions options) : base(options)
        {
            //this.Set<AlgoParam>().AsNoTracking();
            //this.Set<AlgoProfile>().AsNoTracking();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<FZOrderEnt>().HasAlternateKey(t => t.Name);
            builder.Entity<FZOrderEnt>().Property(e => e.Order).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<FZOrder>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }



    public class OrderDbContext2 : DbContext
    {
        public DbSet<FZOrderEnt2> Orders { get; set; }

        public OrderDbContext2(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<FZOrderEnt2>().HasKey(t => t.Id);
            builder.Entity<FZOrderEnt2>().HasAlternateKey(t => t.Name);
            //builder.Entity<FZOrderEnt2>().Property(t => t.Exchange).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.Symbol).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.PrevRefId).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.RefId).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.AlgoId).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.OrderMode).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.PxFormat).IsRequired(false);
            //builder.Entity<FZOrderEnt2>().Property(t => t.AmtFormat).IsRequired(false);


            //            public DateTime LocalTime { get { return TimeCreated.ToLocalTime(); } }
            //public string Id { get { return OrderId; } }
            //public string Exchange { get; set; }
            //public string OrderId { get; set; }
            //public DateTime TimeCreated { get; set; }
            //public DateTime TimeLast { get; set; }
            //public string Symbol { get; set; }
            //public double Amount { get; set; }
            //public double DealAmount { get; set; }
            //public double AvgPrice { get; set; }
            //public double Price { get; set; }
            //public double TriggerPrice { get; set; }
            //public double CommissionPaid { get; set; }
            //public OrderState State { get; set; }
            //public OrderType Ordertype { get; set; }
            ////public TimeInForceType TimeInForce { get; set; }
            //public string PrevRefId { get; set; }
            //public string RefId { get; set; }

            //public string AlgoId { get; set; }
            //public double TPTriggerPrice { get; set; }
            //public double TPPrice { get; set; }
            //public double SLTriggerPrice { get; set; }
            //public double SLPrice { get; set; }
            //public double MinSz { get; set; } = 1;
            //public double TickSz { get; set; }

            //public string OrderMode { get; set; }
            //public bool IsMarginOrder { get; set; }

            //public string PxFormat { get; set; }
            //public string AmtFormat { get; set; }

            //public ExecutionType ExecutionType { get; set; } = ExecutionType.Standard;

        }
    }
}