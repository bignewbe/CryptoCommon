using CryptoCommon.DataBase.Entity;
using CryptoCommon.DataBase.Interface;
using CryptoCommon.DataTypes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoCommonUnitTest
{
    internal class ApplicationDbContext : DbContext
    {
        public DbSet<KeyValueEnt<string>> AppSettings { get; set; }
        public DbSet<KeyValueEnt<FZOrder>> Orders { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<KeyValueEnt<string>>().HasAlternateKey(t => t.Name);
            builder.Entity<KeyValueEnt<FZOrder>>().HasAlternateKey(t => t.Name);
            builder.Entity<KeyValueEnt<FZOrder>>().Property(e => e.Content).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<FZOrder>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));


            //builder.Entity<ProductEnt>().HasMany(t => t.PickLocations).WithOne(t => t.Product).OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<ActionGroupEnt>().Property(e => e.PickToPlaceRotation).HasConversion(
            //    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
            //    v => JsonConvert.DeserializeObject<Dictionary<int, double>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            //builder.Entity<ActionConfig>().HasOne(a => a.ProductForMinimumSize).WithMany(p => p.ActionConfigs);
            //builder.Entity<ActionProductRelation>().HasOne(ap => ap.ActionConfig).WithMany(a => a.ActionProductMinSize).HasForeignKey(ap => ap.ActionId);
            //builder.Entity<ActionProductRelation>().HasOne(ap => ap.Product).WithMany(a => a.ActionProductMinSize).HasForeignKey(ap => ap.ProductId);
            //.Metadata.SetValueComparer(new ValueComparer<List<List<double>>>(
            //(c1, c2) => c1.SequenceEqual(c2),
            //c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            //c => c.ToHashSet());
            //builder.Entity<AlgoParam>().HasAlternateKey(u => u.Name);
            ////builder.Entity<AlgoParam>().HasIndex(u => u.Name).IsUnique();
            //builder.Entity<AlgoProfile>().HasAlternateKey(u => new { u.AlgoParamId, u.Name});
            //builder.Entity<AlgoParam>().Property(e=>e.JsonStrProfiles)
            //    .HasDefaultValue(new Dictionary<string,string>())
            //    .HasConversion(
            //    v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
            //    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        }
    }
}
