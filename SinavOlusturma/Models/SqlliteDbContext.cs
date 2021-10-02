using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SinavOlusturma.Models
{
    public class SqlliteDbContext : DbContext
    {
        public DbSet<Kullanicilar> Kullanicilar { get; set; }
        public DbSet<Sinavlar> Sinavlar { get; set; }
        public DbSet<Sorular> Sorular { get; set; }
        public DbSet<Cevaplar> Cevaplar { get; set; }
        public DbSet<SoruSecenekleri> SoruSecenekleri { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString: "FileName=konusarakogrensinav", sqliteOptionsAction: option =>
              {
                  option.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                  base.OnConfiguring(optionsBuilder);
              });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kullanicilar>().ToTable("Kullanicilar", "Sinav");
            modelBuilder.Entity<Kullanicilar>(entity =>
            {
                entity.HasKey(m => m.KullaniciId);
            });

            modelBuilder.Entity<Sinavlar>().ToTable("Sinavlar", "Sinav");
            modelBuilder.Entity<Sinavlar>(entity =>
            {
                entity.HasKey(m => m.SinavId);
            });
            modelBuilder.Entity<Sorular>().ToTable("Sorular", "Sinav");
            modelBuilder.Entity<Sorular>(entity =>
            {
                entity.HasKey(m => m.SoruId);
            });
            modelBuilder.Entity<Cevaplar>().ToTable("Cevaplar", "Sinav");
            modelBuilder.Entity<Cevaplar>(entity =>
            {
                entity.HasKey(m => m.CevapId);
            });
            modelBuilder.Entity<SoruSecenekleri>().ToTable("SoruSecenekleri", "Sinav");
            modelBuilder.Entity<SoruSecenekleri>(entity =>
            {
                entity.HasKey(m => m.SoruSecenekleriId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
