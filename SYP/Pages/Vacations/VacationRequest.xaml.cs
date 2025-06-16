using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using MigraDoc.DocumentObjectModel;
using Newtonsoft.Json;
using SYP.Context;
using SYP.Models;
using SYP.Models.Holiday;
using SYP.Pages.Departments;
using SYP.Pages.Employees;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Document.NET;
using Xceed.Words.NET;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SYP.Pages.Vacations
{
    /// <summary>
    /// Логика взаимодействия для VacationRequest.xaml
    /// </summary>
    public partial class VacationRequest : System.Windows.Controls.Page
    {
        VacationContext vacationContext = new VacationContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        EmployeeContext employeeContext = new EmployeeContext();
        DepartmentContext departmentContext = new DepartmentContext();
        VacationStatusContext statusContext = new VacationStatusContext();
        Models.Vacations vacations;
        Vacations MainVacations;
        private Users currentUser;
        private int remainingDays = 0;
        private int usedDays = 0;
        private double accruedDays = 0;

        public VacationRequest(Vacations MainVacations, Models.Vacations vacations)
        {
            InitializeComponent();

            this.MainVacations = MainVacations;
            this.vacations = vacations;

            if (vacations != null)
            {
                dateStart.SelectedDate = vacations.StartDate;
                dateEnd.SelectedDate = vacations.EndDate;
                Type.SelectedItem = typeContext.VacationTypes.Where(x => x.Id == vacations.TypeId).FirstOrDefault()?.Name;
            }

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
            {
                Employee.Content = $"{emp.LastName} {emp.FirstName} {emp.Patronymic}";
                var hireDate = emp.HireDate;
                var today = DateTime.Today;
                int monthsWorked = ((today.Year - hireDate.Year) * 12) + today.Month - hireDate.Month;
                monthsWorked = Math.Max(0, monthsWorked);
                accruedDays = Math.Round((monthsWorked / 12.0) * 28, 1);
                var currentYear = DateTime.Now.Year;
                usedDays = vacationContext.Vacations
                    .Where(v => v.EmployeeId == currentUser.EmployeeId && v.StatusId == 2 && v.StartDate.Year == currentYear)
                    .AsEnumerable()
                    .Sum(v => (v.EndDate - v.StartDate).Days + 1);
                remainingDays = (int)Math.Floor(accruedDays - usedDays);
                txtRemainingDays.Text = $"Осталось дней отпуска: {remainingDays}";
            }

            foreach (var type in typeContext.VacationTypes)
                Type.Items.Add(type.Name);
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

            var noDeductTypes = new[] { "Дополнительный", "Без сохранения з/п", "Учебный", "Декретный" };

            if (!noDeductTypes.Contains(selectedType.Name))
            {
                if (vacationDaysWithoutHolidays > remainingDays)
                {
                    MessageBox.Show("Количество рабочих дней отпуска превышает остаток дней!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var emp = employeeContext.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);

            if (IsDepartmentOnVacation(employeeContext, vacationContext, emp.Id, start, end))
            {
                MessageBox.Show("Все сотрудники вашего отдела уже в отпуске в выбранный период. Невозможно оформить отпуск.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (vacations == null)
                {
                    var newVacation = new Models.Vacations
                    {
                        EmployeeId = currentUser.EmployeeId,
                        StartDate = start,
                        EndDate = end,
                        TypeId = selectedType.Id,
                        StatusId = 1
                    };
                    MainVacations.VacationContext.Vacations.Add(newVacation);
                }
                else
                {
                    vacations.StartDate = start;
                    vacations.EndDate = end;
                    vacations.TypeId = selectedType.Id;
                    vacations.StatusId = 1;
                }
                MainVacations.VacationContext.SaveChanges();

                MessageBox.Show("Заявка успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

                int vacationDays = (end - start).Days + 1;

                var holidays = await HolidayService.GetUpcomingHolidaysAsync();
                var holidayDatesInRange = holidays
                    .Where(h => h.Date >= start && h.Date <= end)
                    .Select(h => h.Date)
                    .Distinct()
                    .ToList();

                int holidayCount = holidayDatesInRange.Count;
                int vacationDaysWithoutHolidays = vacationDays - holidayCount;

                var selectedType = Type.SelectedItem as string ?? "";

                bool isCounted = selectedType == "Ежегодный";

                string vacationText = $"{selectedType} отпуск: {vacationDaysWithoutHolidays} дней";

                if (holidayCount > 0)
                    vacationText += $" (без учёта {holidayCount} праздничных дней)";

                if (isCounted)
                {
                    int remainingAfterThis = remainingDays - vacationDaysWithoutHolidays;

                    if (remainingAfterThis < 0)
                    {
                        vacationText += $" (превышен лимит на {Math.Abs(remainingAfterThis)} дней)";
                        txtVacationDaysCount.Foreground = Brushes.Red;
                    }
                    else
                    {
                        txtVacationDaysCount.Foreground = Brushes.Black;
                    }

                    txtRemainingDays.Text = $"Осталось дней отпуска: {remainingAfterThis} (накоплено {remainingDays + usedDays} - использовано {usedDays + vacationDaysWithoutHolidays})";
                }
                else
                {
                    txtVacationDaysCount.Foreground = Brushes.Black;

                    txtRemainingDays.Text = $"Осталось дней отпуска: {remainingDays} (накоплено {remainingDays + usedDays} - использовано {usedDays})";
                }

                txtVacationDaysCount.Text = vacationText;
                txtVacationDaysCount.Visibility = Visibility.Visible;
            }
            else
            {
                txtVacationDaysCount.Visibility = Visibility.Collapsed;
            }
        }
        
        

        private void VacationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = Type.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selectedType))
            {
                dateStart.IsEnabled = false;
                dateEnd.IsEnabled = false;
                txtVacationDaysCount.Visibility = Visibility.Collapsed;
                return;
            }

            dateStart.IsEnabled = true;
            dateEnd.IsEnabled = true;

            txtVacationDaysCount.Text = string.Empty;
            txtVacationDaysCount.Foreground = Brushes.Black;

            switch (selectedType)
            {
                case "Дополнительный":
                    txtVacationDaysCount.Text = "Дополнительный отпуск.";
                    break;
                case "Без сохранения з/п":
                    txtVacationDaysCount.Text = "Отпуск без сохранения зарплаты.";
                    break;
                case "Учебный":
                    txtVacationDaysCount.Text = "Учебный отпуск предоставляется по справке.";
                    break;
            }

            if (!string.IsNullOrEmpty(txtVacationDaysCount.Text))
                txtVacationDaysCount.Visibility = Visibility.Visible;
        }

        private async void DatePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            if (currentUser?.EmployeeId == null) return;

            var emp = await employeeContext.Employees.FirstOrDefaultAsync(emp => emp.Id == currentUser.EmployeeId);
            if (emp == null) return;

            int departmentId = emp.DepartmentId;

            var approvedStatus = await statusContext.VacationStatus
                .FirstOrDefaultAsync(s => s.Name == "Одобрено");

            if (approvedStatus == null) return;

            var allVacations = vacationContext.Vacations
                .Where(v => v.StatusId == approvedStatus.Id)
                .ToList();

            var allEmployees = employeeContext.Employees.ToList();

            if (sender is DatePicker datePicker &&
                datePicker.Template.FindName("PART_Popup", datePicker) is Popup popup &&
                popup.Child is System.Windows.Controls.Calendar calendar)
            {
                await HighlightVacationDates(calendar, departmentId, allVacations, allEmployees);

                // Обработка переключения месяца
                calendar.DisplayDateChanged -= Calendar_DisplayDateChanged; // чтобы не подписаться дважды
                calendar.DisplayDateChanged += Calendar_DisplayDateChanged;

                async void Calendar_DisplayDateChanged(object s, CalendarDateChangedEventArgs args)
                {
                    await HighlightVacationDates(calendar, departmentId, allVacations, allEmployees);
                }
            }
        }

        private async Task HighlightVacationDates(System.Windows.Controls.Calendar calendar, int departmentId, List<Models.Vacations> allVacations, List<Models.Employees> allEmployees)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                await Task.Delay(100);

                var buttons = FindVisualChildren<CalendarDayButton>(calendar);
                foreach (var btn in buttons)
                {
                    if (btn.DataContext is DateTime date)
                    {
                        var employeesOnVacation = allVacations
                            .Where(v => v.StartDate <= date && v.EndDate >= date)
                            .Select(v => allEmployees.FirstOrDefault(empItem => empItem.Id == v.EmployeeId))
                            .Where(employee => employee != null && employee.DepartmentId == departmentId)
                            .ToList();

                        if (employeesOnVacation.Any())
                        {
                            btn.Background = new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#f6d6d6"));
                            var names = employeesOnVacation
                                .Select(empItem => $"{empItem.LastName} {empItem.FirstName} {empItem.Patronymic}")
                                .Distinct();
                            btn.ToolTip = "В отпуске:\n" + string.Join("\n", names);
                        }
                        else
                        {
                            btn.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
                            btn.ToolTip = null;
                        }
                    }
                }
            }, DispatcherPriority.Background);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                        yield return t;

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }
}
