using DividendTracker.Data;
using DividendTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace DividendTracker.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<UserPortfolio> UserPortfolios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPortfolio>()
                .HasOne(up => up.Stock)
                .WithMany(s => s.UserPortfolios) 
                .HasForeignKey(up => up.Ticker); 

            modelBuilder.ApplyConfiguration(new StockConfiguration());
            modelBuilder.ApplyConfiguration(new DividendConfiguration());
            // add the other configs here for the otehr tables.
            base.OnModelCreating(modelBuilder);
        }

    }


}
