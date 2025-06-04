using SYP.Context;
using SYP.Models.Holiday;
using SYP.Models.Weather;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

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

            currentUser = MainWindow.mw.CurrentUser;

            LoadWeather();
            LoadHolidays();
            CheckUpcomingVacation();
            CheckBirthday();

            int newVacationRequestsCount = GetNewVacationRequestsCount();
            bool isAdmin = currentUser.Role == "Admin";

            notificationItem.ShowVacationRequestsNotification(newVacationRequestsCount, isAdmin);

            if (notificationItem.HasNotifications())
            {
                notificationItem.Visibility = Visibility.Visible;

                var fadeIn = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                notificationItem.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
            else
            {
                notificationItem.Visibility = Visibility.Collapsed;
            }

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

            var today = DateTime.Today;

            int activeVacationsCount = VacationContext.Vacations.Count(v => v.EndDate.Date >= today && v.StatusId == 2);

            CountVacation.Content = "В отпуске сейчас: " + activeVacationsCount + $" {GetEmployeeText(activeVacationsCount)}";

        }

        private int GetNewVacationRequestsCount()
        {
            using (var context = new VacationContext())
            {
                return context.Vacations.Count(v => v.StatusId == 1);
            }
        }

        private void CheckBirthday()
        {
            using (var context = new EmployeeContext())
            {
                DateTime today = DateTime.Today;

                var birthdayEmployees = context.Employees
                    .Where(e => e.BirthDate.Day == today.Day && e.BirthDate.Month == today.Month)
                    .ToList();

                if (birthdayEmployees.Any())
                {
                    string names = string.Join(", ", birthdayEmployees.Select(e => $"{e.LastName} {e.FirstName} {e.Patronymic}"));

                    string message;
                    if (birthdayEmployees.Count == 1)
                    {
                        message = $"• Сегодня день рождения у: {names}!";
                    }
                    else
                    {
                        message = $"• Сегодня дни рождения у: {names}!";
                    }

                    notificationItem.ShowBirthdayNotification(message);
                }
                else
                {
                    notificationItem.HideBirthdayNotification();
                }
            }
        }

        private void CheckUpcomingVacation()
        {
            using (var context = new VacationContext())
            {
                var upcomingVacation = context.Vacations.Where(v => v.EmployeeId == currentUser.EmployeeId && v.StartDate.Date >= DateTime.Today && v.StatusId == 2).OrderBy(v => v.StartDate).FirstOrDefault();

                if (upcomingVacation != null)
                {
                    int daysLeft = (upcomingVacation.StartDate.Date - DateTime.Today).Days;

                    if (daysLeft <= 14)
                    {
                        string message;
                        message = $"• Ваш отпуск начнется через {daysLeft} {GetDayWord(daysLeft)} — {upcomingVacation.StartDate:dd.MM.yyyy}";
                        notificationItem.ShowVacationNotification(message);
                    }
                    else
                    {
                        notificationItem.HideVacationNotification();
                    }
                }
                else
                {
                    notificationItem.HideVacationNotification();
                }
            }
        }

        private string GetDayWord(int days)
        {
            if (days % 10 == 1 && days % 100 != 11)
                return "день";
            else if (days % 10 >= 2 && days % 10 <= 4 && (days % 100 < 10 || days % 100 >= 20))
                return "дня";
            else
                return "дней";
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
