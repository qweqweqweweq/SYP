using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models.Holiday
{
    public class Holiday
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }

        public string FormattedDate { get; set; }
    }

    public class CalendarData
    {
        public int Year { get; set; }
        public List<Holiday> Holidays { get; set; }
        public List<Holiday> ShortDays { get; set; }
        public int Status { get; set; }
    }
}
