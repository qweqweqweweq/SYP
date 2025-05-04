using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SYP.Models.Holiday
{
    public static class HolidayService
    {
        public static async Task<List<Holiday>> GetUpcomingHolidaysAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://calendar.kuzyak.in/api/calendar/2025/holidays");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var calendarData = JsonConvert.DeserializeObject<CalendarData>(json);

                    return calendarData?.Holidays
                        .Where(h => h.Date >= DateTime.Now)
                        .OrderBy(h => h.Date)
                        .Take(3)
                        .ToList();
                }
                return new List<Holiday>();
            }
        }
    }
}
