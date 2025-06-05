using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SYP.Models.Calendar
{
    public class DayOfWeekBrush : IValueConverter
    {
        public Brush HolidayBrush { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f6d6d6"));
        public Brush WeekendBrush { get; set; } = Brushes.LightCoral;
        public Brush DefaultBrush { get; set; } = Brushes.Transparent;

        private HashSet<DateTime> Holidays { get; set; } = new();

        public void SetHolidays(IEnumerable<DateTime> holidayDates)
        {
            Holidays = new HashSet<DateTime>(holidayDates.Select(d => d.Date));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                if (Holidays.Contains(date.Date))
                    return HolidayBrush;

                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    return WeekendBrush;
            }

            return DefaultBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
