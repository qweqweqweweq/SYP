using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SYP.Context;
using SYP.Models;
using SYP.Models.Holiday;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SYP.Pages.Vacations
{
    /// <summary>
    /// Логика взаимодействия для VacationRequest.xaml
    /// </summary>
    public partial class VacationRequest : Page
    {
        VacationContext vacationContext = new VacationContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        EmployeeContext employeeContext = new EmployeeContext();
        Models.Vacations vacations;
        private Users currentUser;
        private int remainingDays = 0;

        public VacationRequest()
        {
            InitializeComponent();

            Loaded += VacationRequest_Loaded;
        }

        private async void VacationRequest_Loaded(object sender, RoutedEventArgs e)
        {
            await UpdateEmployeeVacationBalancesAsync();

            currentUser = MainWindow.mw.CurrentUser;

            if (currentUser == null || currentUser.EmployeeId == null)
            {
                MessageBox.Show("Ошибка: невозможно определить текущего сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var emp = employeeContext.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);
            if (emp != null)
                Employee.Content = $"{emp.LastName} {emp.FirstName} {emp.Patronymic}";

            foreach (var type in typeContext.VacationTypes)
                Type.Items.Add(type.Name);

            var currentYear = DateTime.Now.Year;
            var usedDays = vacationContext.Vacations
                .Where(v => v.EmployeeId == currentUser.EmployeeId && v.StatusId == 2 && v.StartDate.Year == currentYear)
                .AsEnumerable()
                .Sum(v => (v.EndDate - v.StartDate).Days + 1);

            remainingDays = 28 - usedDays;
            txtRemainingDays.Text = $"Осталось дней отпуска: {remainingDays}";
        }

        public async Task<int> GetHolidayCountInRange(DateTime start, DateTime end)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"https://calendar.kuzyak.in/api/calendar/{start.Year}/holidays");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var calendarData = JsonConvert.DeserializeObject<CalendarData>(json);

                    if (calendarData?.Holidays == null)
                        return 0;

                    return calendarData.Holidays.Count(h => h.Date >= start && h.Date <= end);
                }
            }
            return 0;
        }

        public async Task UpdateEmployeeVacationBalancesAsync()
        {
            var employees = await employeeContext.Employees.ToListAsync();

            int previousYear = DateTime.Now.Year - 1;
            var vacations = await vacationContext.Vacations
                .Where(v => v.StartDate.Year == previousYear)
                .ToListAsync();

            foreach (var employee in employees)
            {
                var employeeVacations = vacations.Where(v => v.EmployeeId == employee.Id);

                int usedDaysThisYear = employeeVacations.Sum(v => (v.EndDate - v.StartDate).Days + 1);

                int carriedOver = Math.Max(0, 28 - usedDaysThisYear);
            }

            await employeeContext.SaveChangesAsync();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Vacations());
        }

        private async void SendRequest(object sender, RoutedEventArgs e)
        {
            if (dateStart.SelectedDate == null || dateEnd.SelectedDate == null || Type.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dateStart.SelectedDate > dateEnd.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedType = typeContext.VacationTypes.FirstOrDefault(x => x.Name == Type.SelectedItem.ToString());
            if (selectedType == null)
            {
                MessageBox.Show("Выбран некорректный тип отпуска.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var start = dateStart.SelectedDate.Value;
            var end = dateEnd.SelectedDate.Value;

            int totalDays = (end - start).Days + 1;
            int holidayCount = await GetHolidayCountInRange(start, end);
            int vacationDaysWithoutHolidays = totalDays - holidayCount;

            if (vacationDaysWithoutHolidays > remainingDays)
            {
                MessageBox.Show("Количество рабочих дней отпуска превышает остаток дней!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var emp = employeeContext.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);

            if (IsDepartmentOnVacation(employeeContext, vacationContext, emp.Id, start, end))
            {
                MessageBox.Show("Все сотрудники вашего отдела уже в отпуске в выбранный период. Невозможно оформить отпуск.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newVacation = new Models.Vacations
            {
                EmployeeId = currentUser.EmployeeId,
                StartDate = start,
                EndDate = end,
                TypeId = selectedType.Id,
                StatusId = 1
            };

            try
            {
                vacationContext.Vacations.Add(newVacation);
                vacationContext.SaveChanges();

                await UpdateEmployeeVacationBalancesAsync();

                MessageBox.Show("Заявка успешно отправлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.mw.OpenPages(new Vacations());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: \n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsDepartmentOnVacation(EmployeeContext empContext, VacationContext vacContext, int employeeId, DateTime start, DateTime end)
        {
            var employee = empContext.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
                return false;

            int departmentId = employee.DepartmentId;

            var departmentEmployees = empContext.Employees
                .Where(e => e.DepartmentId == departmentId)
                .Select(e => e.Id)
                .ToList();

            int approvedStatusId = 2;

            var overlappingVacationsCount = vacContext.Vacations
                .Where(v => v.StatusId == approvedStatusId
                    && departmentEmployees.Contains(v.EmployeeId)
                    && (
                        (start >= v.StartDate && start <= v.EndDate) ||
                        (end >= v.StartDate && end <= v.EndDate) ||
                        (start <= v.StartDate && end >= v.EndDate)
                    ))
                .Select(v => v.EmployeeId)
                .Distinct()
                .Count();

            int totalEmployeesInDepartment = departmentEmployees.Count;

            return overlappingVacationsCount >= totalEmployeesInDepartment - 1;
        }

        private async void Date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dateEnd.SelectedDate.HasValue)
                txtVacationDaysCount.Visibility = Visibility.Visible;

            if (dateStart.SelectedDate.HasValue)
            {
                dateEnd.DisplayDateStart = dateStart.SelectedDate;

                if (dateEnd.SelectedDate.HasValue && dateEnd.SelectedDate < dateStart.SelectedDate)
                {
                    dateEnd.SelectedDate = dateStart.SelectedDate;
                }
            }
            else
            {
                dateEnd.DisplayDateStart = null;
            }

            if (dateStart.SelectedDate.HasValue && dateEnd.SelectedDate.HasValue)
            {
                var start = dateStart.SelectedDate.Value;
                var end = dateEnd.SelectedDate.Value;

                if (end >= start)
                {
                    int vacationDays = (end - start).Days + 1;

                    var holidays = await HolidayService.GetUpcomingHolidaysAsync();
                    var holidayDatesInRange = holidays
                        .Where(h => h.Date >= start && h.Date <= end)
                        .Select(h => h.Date)
                        .Distinct()
                        .ToList();

                    int holidayCount = holidayDatesInRange.Count;
                    int vacationDaysWithoutHolidays = vacationDays - holidayCount;

                    string vacationText = $"Количество дней отпуска: {vacationDaysWithoutHolidays}";
                    if (holidayCount > 0)
                        vacationText += $" (без учёта {holidayCount} праздничных дней)";

                    txtVacationDaysCount.Text = vacationText;
                    txtVacationDaysCount.Visibility = Visibility.Visible;

                    if (vacationDaysWithoutHolidays > remainingDays)
                    {
                        txtVacationDaysCount.Text += " (превышен лимит!)";
                        txtVacationDaysCount.Foreground = Brushes.Red;
                    }
                    else
                    {
                        txtVacationDaysCount.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    txtVacationDaysCount.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                txtVacationDaysCount.Visibility = Visibility.Collapsed;
            }
        }
    }
}
