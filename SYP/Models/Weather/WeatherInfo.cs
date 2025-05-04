using SYP.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SYP.Models.Weather
{
    public class WeatherInfo
    {
        public Main main { get; set; }
        public List<Weather> weather { get; set; }
        public string name { get; set; }
    }
    public class Main
    {
        public double temp { get; set; }
    }
    public class Weather
    {
        public string description { get; set; }
        public string icon { get; set; }
    }
}
