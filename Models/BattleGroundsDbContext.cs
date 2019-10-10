using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleGrounds.Models
{
    public class BattleGroundsDbContext : DbContext
    {
        public BattleGroundsDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Army> Armies { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<GameOptions> GameOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
