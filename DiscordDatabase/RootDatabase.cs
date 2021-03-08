using System;
using System.IO;

using Microsoft.EntityFrameworkCore;

namespace DiscordDatabase
{
    public class RootDatabase : DbContext
    {
        public DbSet<User> Users { get; set; }

        public RootDatabase()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string baseDir = Path.Combine(AppContext.BaseDirectory, "Common/Data");
            string datadir = Path.Combine(baseDir, "Database.sqlite");
            
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
            }

            optionsBuilder.UseSqlite($"Data Source={datadir}");
        }
    }
}