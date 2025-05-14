using SYP.Context;
using SYP.Models.Holiday;
using SYP.Models.Weather;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYP.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        EmployeeContext EmployeeContext = new EmployeeContext();
        DepartmentContext DepartmentContext = new DepartmentContext();
        VacationContext VacationContext = new VacationContext();
        PositionContext PositionContext = new PositionContext();
        Models.Users currentUser;

        public Main()
        {
            InitializeComponent();

            LoadWeather();
            LoadHolidays();

            var currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                settings.Visibility = Visibility.Hidden;
            }

            using (var context = new EmployeeContext())
            {
                var employee = context.Employees.FirstOrDefault(x => x.Id == currentUser.EmployeeId);
                lbWelcome.Content = $"Добро пожаловать, {employee?.FirstName}";
            }

            using (var deptCtx = new DepartmentContext())
            {
                CountDepartments.Content = "Всего отделов: " + deptCtx.Departments.Count();
            }
            using (var emplCtx = new EmployeeContext())
            {
                CountEmployees.Content = "Всего сотрудников: " + emplCtx.Employees.Count();
            }

            var countPositions = PositionContext.Positions.Count();
            var occupiedPositions = EmployeeContext.Employees.Select(e => e.PositionId).Distinct().Count();
            var freePositions = countPositions - occupiedPositions;

            CountFreePositions.Content = "Свободные должности: " + freePositions;
            CountVacation.Content = "В отпуске сейчас: " + VacationContext.Vacations.Count() + $" {GetEmployeeText(VacationContext.Vacations.Count())}";

        }

        private string GetEmployeeText(int count)
        {
            if (count % 10 == 1 && count % 100 != 11)
                return "сотрудник";
            else if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20))
                return "сотрудника";
            else
                return "сотрудников";
        }

        private async void LoadWeather()
        {
            try
            {
                var weather = await WeatherService.GetWeatherAsync();

                string description = weather.weather[0].description.ToLower();
                string emoji = GetWeatherEmoji(description);

                WeatherTemp.Text = $"{Math.Round(weather.main.temp)}°C, {weather.name}";
                WeatherDesc.Text = $"{emoji} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(description)}";
            }
            catch
            {
                WeatherTemp.Text = "Ошибка загрузки";
                WeatherDesc.Text = "";
            }
        }

        private string GetWeatherEmoji(string description)
        {
            if (description.Contains("ясно")) return "☀️";
            if (description.Contains("облачно") || description.Contains("пасмурно")) return "☁️";
            if (description.Contains("дождь")) return "🌧️";
            if (description.Contains("гроза")) return "⛈️";
            if (description.Contains("снег")) return "❄️";
            if (description.Contains("туман") || description.Contains("дымка")) return "🌫️";

            return "🌡️";
        }

        private async void LoadHolidays()
        {
            try
            {
                var holidays = await HolidayService.GetUpcomingHolidaysAsync();
                HolidayList.ItemsSource = holidays;
            }
            catch
            {
                HolidayList.ItemsSource = new List<Holiday>
                {
                    new Holiday { Name = "Ошибка загрузки", Date = DateTime.Now }
                };
            }
        }


        private void OpenEmployees(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees.Employees());
        }

        private void OpenDepartments(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Departments.Departments());
        }

        private void OpenPositions(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Positions.Positions());
        }

        private void OpenVacations(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Settings());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Authorization.Authorization());
        }
    }
}
