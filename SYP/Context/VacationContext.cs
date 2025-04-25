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
    public class VacationContext : DbContext
    {
        public DbSet<Vacations> Vacations { get; set; }
        public VacationContext()
        {
            Database.EnsureCreated();
            Vacations.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
