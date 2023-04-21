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
}