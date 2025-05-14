using Microsoft.EntityFrameworkCore;
using SYP.Context.DB;
using SYP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Context
{
    public class EmployeeStatusContext : DbContext
    {
        public DbSet<EmployeeStatus> Status { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }

        public void MigrateDatabase()
        {
            Database.Migrate();
        }
    }
}
