using SYP.Models.Calendar;
using SYP.Models.Holiday;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SYP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var holidays = await HolidayService.GetUpcomingHolidaysAsync();

            if (Current.Resources["WeekendBrushConverter"] is DayOfWeekBrush converter)
            {
                converter.SetHolidays(holidays.Select(h => h.Date));
            }
        }
    }

}
