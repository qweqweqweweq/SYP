using DocumentFormat.OpenXml.InkML;
using MigraDoc.DocumentObjectModel;
using Newtonsoft.Json;
using SYP.Context;
using SYP.Models.Holiday;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Words.NET;

namespace SYP.Pages.Vacations
{
    /// <summary>
    /// Логика взаимодействия для VacationEdit.xaml
    /// </summary>
    public partial class VacationEdit : Page
    {
        Vacations MainVacations;
        Models.Vacations vacations;


        EmployeeContext employeeContext = new EmployeeContext();
        VacationContext vacationContext = new VacationContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        VacationStatusContext statusContext = new VacationStatusContext();
        DepartmentContext departmentContext = new DepartmentContext();

        private Models.Users currentUser;

        private int remainingDays = 0;
        private int usedDays = 0;
        private double accruedDays = 0;

        public VacationEdit(Vacations MainVacations, Models.Vacations vacations)
        {
            InitializeComponent();

            currentUser = MainWindow.mw.CurrentUser;

            this.MainVacations = MainVacations;
            this.vacations = vacations;

            foreach (var item in employeeContext.Employees)
                Employee.Items.Add(item.LastName + " " + item.FirstName + " " + item.Patronymic);

            foreach (var item in typeContext.VacationTypes)
                Type.Items.Add(item.Name);

            foreach (var item in statusContext.VacationStatus)
                Status.Items.Add(item.Name);

            Employee.SelectionChanged += Employee_SelectionChanged;
            Employee.SelectionChanged -= Employee_SelectionChanged;

            dateStart.SelectedDateChanged += Date_SelectedDateChanged;
            dateEnd.SelectedDateChanged += Date_SelectedDateChanged;

            if (vacations != null)
            {
                lbTitle.Content = "Изменение существующего отпуска";

                var selectedEmployee = employeeContext.Employees.FirstOrDefault(x => x.Id == vacations.EmployeeId);
                if (selectedEmployee != null)
                {
                    Employee.SelectedItem = selectedEmployee.LastName + " " + selectedEmployee.FirstName + " " + selectedEmployee.Patronymic;
                }

                dateStart.SelectedDate = vacations.StartDate;
                dateEnd.SelectedDate = vacations.EndDate;
                Type.SelectedItem = typeContext.VacationTypes.Where(x => x.Id == vacations.TypeId).FirstOrDefault()?.Name;
                Status.SelectedItem = statusContext.VacationStatus.Where(x => x.Id == vacations.StatusId).FirstOrDefault()?.Name;
            }
        }

        private int GetUsedVacationDaysForYear(Models.Employees employee, int year)
        {
            var approvedStatus = statusContext.VacationStatus.FirstOrDefault(s => s.Name == "Одобрено");
            if (approvedStatus == null)
                return 0;

            int approvedStatusId = approvedStatus.Id;

            return vacationContext.Vacations.Where(v => v.EmployeeId == employee.Id && v.StartDate.Year == year && v.StatusId == approvedStatusId).Sum(v => (v.EndDate - v.StartDate).Days + 1);
        }

        private int GetRemainingVacationDays(Models.Employees employee, int year, int totalDaysPerYear = 28)
        {
            var validStatuses = statusContext.VacationStatus.Where(s => s.Name == "Одобрено" || s.Name == "Завершён").Select(s => s.Id).ToList();

            var vacations = vacationContext.Vacations
                .Where(v => v.EmployeeId == employee.Id
                            && v.StartDate.Year == year
                            && validStatuses.Contains(v.StatusId))
                .ToList();

            int totalUsedDays = vacations.Sum(v => (v.EndDate - v.StartDate).Days + 1);

            return totalDaysPerYear - totalUsedDays;
        }

        private void Employee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtVacationDaysLeft.Visibility = Visibility.Visible;

            if (Employee.SelectedItem != null)
            {
                var selectedFullName = Employee.SelectedItem.ToString();
                var employee = employeeContext.Employees.FirstOrDefault(x =>
                    (x.LastName + " " + x.FirstName + " " + x.Patronymic) == selectedFullName);

                if (employee != null)
                {
                    if (!CanRequestVacation(employee))
                    {
                        Employee.SelectedIndex = -1;
                        txtVacationDaysLeft.Visibility = Visibility.Collapsed;
                        MessageBox.Show("У сотрудника не прошло 6 месяцев после трудоустройства. Отпуск пока невозможен.",
                            "Отказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var hireDate = employee.HireDate;
                    var today = DateTime.Today;
                    int monthsWorked = ((today.Year - hireDate.Year) * 12) + today.Month - hireDate.Month;
                    monthsWorked = Math.Max(0, monthsWorked);

                    accruedDays = Math.Round((monthsWorked / 12.0) * 28, 1);

                    var currentYear = DateTime.Now.Year;
                    usedDays = vacationContext.Vacations
                        .Where(v => v.EmployeeId == employee.Id && v.StatusId == 2 && v.StartDate.Year == currentYear)
                        .AsEnumerable()
                        .Sum(v => (v.EndDate - v.StartDate).Days + 1);

                    remainingDays = (int)Math.Floor(accruedDays - usedDays);

                    txtVacationDaysLeft.Text = $"Осталось дней отпуска: {remainingDays}";
                }
                else
                {
                    txtVacationDaysLeft.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                txtVacationDaysLeft.Visibility = Visibility.Collapsed;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
        }

        private bool IsDepartmentOnVacation(EmployeeContext employeeContext, VacationContext vacationContext, int employeeId, DateTime start, DateTime end, int? currentVacationId = null)
        {
            var employee = employeeContext.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null) return false;

            var departmentEmployees = employeeContext.Employees.Where(e => e.DepartmentId == employee.DepartmentId).ToList();
            if (departmentEmployees.Count == 0) return false;

            var approvedStatus = statusContext.VacationStatus.FirstOrDefault(s => s.Name == "Одобрено");
            if (approvedStatus == null) return false;

            int approvedStatusId = approvedStatus.Id;

            int countOnVacation = 0;
            foreach (var deptEmp in departmentEmployees)
            {
                bool isOnVacation = vacationContext.Vacations.Any(v =>
                    v.EmployeeId == deptEmp.Id &&
                    v.StatusId == approvedStatusId &&
                    v.Id != currentVacationId &&
                    (
                        (start >= v.StartDate && start <= v.EndDate) ||
                        (end >= v.StartDate && end <= v.EndDate) ||
                        (start <= v.StartDate && end >= v.EndDate)
                    ));
                if (isOnVacation)
                    countOnVacation++;
            }

            return countOnVacation >= departmentEmployees.Count - 1;
        }

        private async void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dateStart.SelectedDate == null || dateEnd.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите дату начала и дату окончания отпуска.");
                    return;
                }

                DateTime start = dateStart.SelectedDate.Value;
                DateTime end = dateEnd.SelectedDate.Value;
                if (end < start)
                {
                    MessageBox.Show("Дата окончания отпуска не может быть раньше даты начала.");
                    return;
                }

                if (Employee.SelectedItem == null)
                {
                    MessageBox.Show("Выберите сотрудника.");
                    return;
                }

                var employeeFullName = Employee.SelectedItem.ToString();
                var employee = employeeContext.Employees.FirstOrDefault(x =>
                    (x.LastName + " " + x.FirstName + " " + x.Patronymic) == employeeFullName);
                if (employee == null)
                {
                    MessageBox.Show("Не удалось найти сотрудника. Проверьте правильность выбора.");
                    return;
                }

                if (Type.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип отпуска.");
                    return;
                }

                var vacationType = typeContext.VacationTypes.FirstOrDefault(x => x.Name == Type.SelectedItem.ToString());
                if (vacationType == null)
                {
                    MessageBox.Show("Тип отпуска не найден.");
                    return;
                }

                if (Status.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус отпуска.");
                    return;
                }

                var vacationStatus = statusContext.VacationStatus.FirstOrDefault(x => x.Name == Status.SelectedItem.ToString());
                if (vacationStatus == null)
                {
                    MessageBox.Show("Статус отпуска не найден.");
                    return;
                }

                if (IsDepartmentOnVacation(employeeContext, vacationContext, employee.Id, start, end, vacations?.Id))
                {
                    MessageBox.Show("Все сотрудники отдела уже находятся в отпуске в выбранный период. Невозможно оформить отпуск.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? currentVacationId = vacations?.Id;
                bool hasOverlap = vacationContext.Vacations.Any(v => v.EmployeeId == employee.Id && v.Id != currentVacationId &&
                (
                    (start >= v.StartDate && start <= v.EndDate) ||
                    (end >= v.StartDate && end <= v.EndDate) ||
                    (start <= v.StartDate && end >= v.EndDate)
                ));

                if (hasOverlap)
                {
                    MessageBox.Show("У сотрудника уже есть отпуск в указанный период.");
                    return;
                }

                int totalDays = (end - start).Days + 1;
                int holidayCount = await GetHolidayCountInRange(start, end);
                int vacationDaysWithoutHolidays = totalDays - holidayCount;

                var noCountTypes = new[] { "Дополнительный", "Без сохранения з/п", "Учебный" };

                if (!noCountTypes.Contains(vacationType.Name) && vacationDaysWithoutHolidays > remainingDays)
                {
                    MessageBox.Show("Количество рабочих дней отпуска превышает остаток дней!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isNew = vacations == null;

                if (isNew)
                {
                    vacations = new Models.Vacations()
                    {
                        EmployeeId = employee.Id,
                        StartDate = start,
                        EndDate = end,
                        TypeId = vacationType.Id,
                        StatusId = vacationStatus.Id
                    };
                    MainVacations.VacationContext.Vacations.Add(vacations);
                }
                else
                {
                    vacations.EmployeeId = employee.Id;
                    vacations.StartDate = start;
                    vacations.EndDate = end;
                    vacations.TypeId = vacationType.Id;
                    vacations.StatusId = vacationStatus.Id;
                }

                MainVacations.VacationContext.SaveChanges();
                MessageBox.Show("Отпуск успешно сохранён.");

                if (!string.IsNullOrEmpty(employee.Email))
                {
                    try
                    {
                        string employeeName = $"{employee.LastName} {employee.FirstName} {employee.Patronymic}";

                        string managerName = "Ваш руководитель";
                        var admin = employeeContext.Employees.FirstOrDefault(x => x.Id == currentUser.EmployeeId);
                        if (admin != null)
                        {
                            managerName = $"{admin.LastName} {admin.FirstName} {admin.Patronymic}";
                        }

                        int departmentId = employee.DepartmentId;
                        var departments = departmentContext.Departments.ToList();

                        string subject = $"{(vacationStatus.Name == "Одобрено" ? "Одобрено: отпуск" : "Изменение отпуска")}";
                        string body = $"Здравствуйте, {employee.FirstName}!\n\n" +
                                      $"Ваш {(isNew ? "новый" : "обновлённый")} отпуск:\n" +
                                      $"- Тип: {vacationType.Name}\n" +
                                      $"- Период: с {start:dd.MM.yyyy} по {end:dd.MM.yyyy}\n" +
                                      $"- Статус: {vacationStatus.Name}\n\n";

                        string? attachmentPath = null;

                        if (vacationStatus.Name == "Одобрено")
                        {
                            attachmentPath = GenerateVacationRequestDoc(
                                managerName,
                                employeeName,
                                departmentId,
                                departments,
                                vacationType.Name,
                                start,
                                end,
                                totalDays
                            );
                            body += "Во вложении — заявление на подпись.";
                        }

                        SendMail(employee.Email, subject, body, attachmentPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при отправке письма сотруднику: " + ex.Message, "Почта", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка при сохранении отпуска.\n" + ex.Message);
            }
        }

        private bool CanRequestVacation(Models.Employees employee)
        {
            return DateTime.Now >= employee.HireDate.AddMonths(6);
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
                DateTime start = dateStart.SelectedDate.Value;
                DateTime end = dateEnd.SelectedDate.Value;

                if (end < start)
                {
                    txtVacationDaysCount.Visibility = Visibility.Collapsed;
                    return;
                }

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
                var noCountTypes = new[] { "Дополнительный", "Без сохранения з/п", "Учебный" };
                bool isCounted = !noCountTypes.Contains(selectedType);

                string vacationText = $"{selectedType} отпуск: {vacationDaysWithoutHolidays} дней";
                if (holidayCount > 0)
                    vacationText += $" (без учёта {holidayCount} праздничных дней)";

                txtVacationDaysCount.Text = vacationText;
                txtVacationDaysCount.Visibility = Visibility.Visible;

                if (isCounted)
                {
                    int remainingAfterThis = remainingDays - vacationDaysWithoutHolidays;

                    if (remainingAfterThis < 0)
                    {
                        txtVacationDaysCount.Text += $" (превышен лимит на {Math.Abs(remainingAfterThis)} дней)";
                        txtVacationDaysCount.Foreground = Brushes.Red;
                    }
                    else
                    {
                        txtVacationDaysCount.Foreground = Brushes.Black;
                    }

                    txtVacationDaysLeft.Text = $"Осталось дней отпуска: {remainingAfterThis} (накоплено {remainingDays + usedDays} - использовано {usedDays + vacationDaysWithoutHolidays})";
                }
                else
                {
                    txtVacationDaysCount.Foreground = Brushes.Black;
                    txtVacationDaysLeft.Text = $"Осталось дней отпуска: {remainingDays} (накоплено {remainingDays + usedDays} - использовано {usedDays})";
                }
            }
            else
            {
                txtVacationDaysCount.Visibility = Visibility.Collapsed;
                txtVacationDaysCount.Foreground = Brushes.Black;
            }
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = Type.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedType))
            {
                dateStart.IsEnabled = false;
                txtVacationDaysCount.Visibility = Visibility.Collapsed;
                dateEnd.IsEnabled = false;
                return;
            }

            dateStart.IsEnabled = true;
            dateEnd.IsEnabled = true;

            switch (selectedType)
            {
                case "Дополнительный":
                    txtVacationDaysCount.Text = "Дополнительный отпуск.";
                    txtVacationDaysCount.Visibility = Visibility.Visible;
                    break;
                case "Без сохранения з/п":
                    txtVacationDaysCount.Text = "Отпуск без сохранения зарплаты.";
                    txtVacationDaysCount.Visibility = Visibility.Visible;
                    break;
                case "Учебный":
                    txtVacationDaysCount.Text = "Учебный отпуск предоставляется по справке.";
                    txtVacationDaysCount.Visibility = Visibility.Visible;
                    break;
                default:
                    txtVacationDaysCount.Text = string.Empty;
                    txtVacationDaysCount.Visibility = Visibility.Collapsed;
                    break;
            }

            if (!string.IsNullOrEmpty(txtVacationDaysCount.Text))
                txtVacationDaysCount.Visibility = Visibility.Visible;
        }

        //email

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

        public void SendMail(string toEmail, string subject, string body, string attachmentPath)
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
    }
}
