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
    public class UserContext : DbContext
    {
        public DbSet<Users> Users { get; set; }
        public UserContext()
        {
            Database.EnsureCreated();
            Users.Load();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(Config.connection, Config.version);
        }
    }
}
