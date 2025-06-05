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
using System.Windows.Media;
using Xceed.Document.NET;
using Xceed.Words.NET;

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
        DepartmentContext departmentContext = new DepartmentContext();
        Models.Vacations vacations;
        private Users currentUser;
        private int remainingDays = 0;
        private int usedDays = 0;
        private double accruedDays = 0;

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
                await vacationContext.SaveChangesAsync();

                await UpdateEmployeeVacationBalancesAsync();

                // Данные администратора
                var manager = employeeContext.Employees.FirstOrDefault(m => m.Id == 6);
                string managerName = manager != null ? $"{manager.LastName} {manager.FirstName} {manager.Patronymic}" : "Руководитель не найден";

                string employeeName = $"{emp.LastName} {emp.FirstName} {emp.Patronymic}";
                string vacationTypeName = selectedType.Name;
                int vacationDaysCount = vacationDaysWithoutHolidays;
                var departments = departmentContext.Departments.ToList();

                string docPath = GenerateVacationRequestDoc(managerName, employeeName, emp.DepartmentId, departments, vacationTypeName, start, end, vacationDaysCount);

                SendEmailWithAttachment(
                    toEmail: manager?.Email,
                    subject: "Заявка на отпуск",
                    body: $"Здравствуйте, {managerName}.\n\nПрикреплена заявка на отпуск сотрудника {employeeName} ({emp.Email}).",
                    attachmentPath: docPath);

                MessageBox.Show("Заявка успешно отправлена и документ отправлен на почту!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.mw.OpenPages(new Vacations());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении или отправке: \n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        string GetDepartmentNameById(int departmentId, List<Models.Departments> departments)
        {
            var department = departments.FirstOrDefault(d => d.Id == departmentId);
            return department != null ? department.Name : "Неизвестный отдел";
        }

        public string GenerateVacationRequestDoc(string managerName, string employeeName, int departmentId, List<Models.Departments> departments, string vacationType, DateTime startDate, DateTime endDate, int daysCount)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), $"VacationRequest_{employeeName}_{DateTime.Now:yyyyMMddHHmmss}.docx");
            var russianCulture = new CultureInfo("ru-RU");
            string formattedDate = DateTime.Today.ToString("d MMMM yyyy", russianCulture);
            string departmentName = GetDepartmentNameById(departmentId, departments);

            using (var doc = DocX.Create(tempFilePath))
            {
                var p1 = doc.InsertParagraph()
                    .AppendLine($"Руководителю: {managerName}")
                    .Font("Times New Roman")
                    .FontSize(14)
                    .AppendLine($"От сотрудника: {employeeName}, отдел {departmentName}")
                    .Font("Times New Roman")
                    .FontSize(14)
                    .SpacingAfter(20);
                p1.Alignment = (Xceed.Document.NET.Alignment)ParagraphAlignment.Right;
                p1.LineSpacing = (20f);

                var p2 = doc.InsertParagraph("Заявление")
                   .Font("Times New Roman")
                   .FontSize(16)
                   .Bold();
                p2.Alignment = (Xceed.Document.NET.Alignment)ParagraphAlignment.Center;
                p2.LineSpacing = (20f);

                var p3 = doc.InsertParagraph()
                    .SpacingAfter(10)
                    .AppendLine($"Тип отпуска: {vacationType}")
                    .Font("Times New Roman")
                    .FontSize(14)
                    .AppendLine($"Прошу предоставить мне отпуск с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy} продолжительностью {daysCount} календарных дней.")
                    .Font("Times New Roman")
                    .FontSize(14);
                p3.SpacingAfter(20);
                p3.LineSpacing = (20f);

                var p4 = doc.InsertParagraph();
                p4.Append(formattedDate)
                    .Font("Times New Roman")
                    .FontSize(14);
                p4.Append("             /Подпись ________/ Расшифровка _________________")
                    .Font("Times New Roman")
                    .FontSize(14);

                p4.LineSpacing = 20f;

                doc.Save();
            }

            return tempFilePath;
        }
        public void SendEmailWithAttachment(string toEmail, string subject, string body, string attachmentPath)
        {
            var fromAddress = new MailAddress("pleshkovaanastas@yandex.ru", "SotrudniK");
            var toAddress = new MailAddress(toEmail);
            const string fromPassword = "enrklmiqltflflhx";

            using (var smtp = new SmtpClient
            {
                Host = "smtp.yandex.ru",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        message.Attachments.Add(new Attachment(attachmentPath));
                    }

                    smtp.Send(message);
                }
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
    }
}
