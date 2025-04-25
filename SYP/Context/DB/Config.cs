using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Context.DB
{
    public class Config
    {
        public static readonly string connection = "server=localhost;port=3306;database=SYP;uid=root;pwd=";
        public static readonly MySqlServerVersion version = new MySqlServerVersion(new Version(8, 0, 11));
    }
}
