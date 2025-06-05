using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using SYP.Context;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using MigraDocDocument = MigraDoc.DocumentObjectModel.Document;
using MigraDocSection = MigraDoc.DocumentObjectModel.Section;
using MigraDoc.DocumentObjectModel.Tables;

namespace SYP.Pages.Vacations
{
    /// <summary>
    /// Логика взаимодействия для Vacations.xaml
    /// </summary>
    public partial class Vacations : Page
    {
        public VacationContext VacationContext = new VacationContext();
        EmployeeContext employeeContext = new EmployeeContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        VacationStatusContext statusContext = new VacationStatusContext();
        private Models.Users currentUser;

        public Vacations()
        {
            InitializeComponent();

            MarkExpiredVacations();
            LoadVacations();

            currentUser = MainWindow.mw.CurrentUser;

            if (currentUser != null && currentUser.Role == "Admin")
            {
                add.Visibility = Visibility.Visible;
                settings.Visibility = Visibility.Hidden;
                export.Visibility = Visibility.Visible;
            }
            else
            {
                request.Visibility = Visibility.Visible;
            }

            foreach (var item in typeContext.VacationTypes) Type.Items.Add(item.Name);

            var pendingCount = VacationContext.Vacations.Count(v => v.StatusId == 1);

            if (currentUser != null && currentUser.Role == "Admin" && pendingCount > 0)
            {
                PendingLabel.Visibility = Visibility.Visible;
            }
        }

        private void LoadVacations()
        {
            if (showVacations == null) return;

            showVacations.Children.Clear();

            var currentUser = MainWindow.mw.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("Пользователь не авторизован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<Models.Vacations> vacations;

            if (currentUser.Role == "Admin")
            {
                vacations = VacationContext.Vacations
                    .OrderBy(v => v.StatusId == 4)
                    .ThenByDescending(v => v.StartDate)
                    .ToList();
            }
            else
            {
                vacations = VacationContext.Vacations
                    .Where(v => v.EmployeeId == currentUser.EmployeeId)
                    .OrderBy(v => v.StatusId == 4)
                    .ThenByDescending(v => v.StartDate)
                    .ToList();
            }

            foreach (var vac in vacations)
            {
                showVacations.Children.Add(new VacationItem(this, vac));
            }
        }

        private void MarkExpiredVacations()
        {
            try
            {
                var today = DateTime.Now.Date;
                var expired = VacationContext.Vacations
                    .Where(v => v.EndDate < today && v.StatusId != 4)
                    .ToList();

                foreach (var vac in expired)
                {
                    vac.StatusId = 4;
                }

                VacationContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении завершённых отпусков: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenMain(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Main());
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

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Settings());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Authorization.Authorization());
        }

        private void AddVacation(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.VacationEdit(this, null));
        }

        private void SelectedType(object sender, SelectionChangedEventArgs e)
        {
            if (Type.SelectedIndex <= 0)
            {
                LoadVacations();
                return;
            }

            string selectedTypeName = Type.SelectedItem.ToString();
            var selectedType = typeContext.VacationTypes.FirstOrDefault(t => t.Name == selectedTypeName);

            if (selectedType != null)
            {
                var matchedVacations = VacationContext.Vacations.Where(v => v.TypeId == selectedType.Id).ToList();

                showVacations.Children.Clear();

                foreach (var item in matchedVacations)
                {
                    showVacations.Children.Add(new VacationItem(this, item));
                }
            }
        }

        private void SearchEmployee(object sender, TextChangedEventArgs e)
        {
            string searchText = search.Text.ToLower();
            var allEmployees = employeeContext.Employees.ToList();

            var matchedEmployees = allEmployees.Where(x => $"{x.LastName} {x.FirstName} {x.Patronymic}".ToLower().Contains(searchText)).Select(x => x.Id).ToList();
            var matchedVacations = VacationContext.Vacations.Where(v => matchedEmployees.Contains(v.EmployeeId)).ToList();

            showVacations.Children.Clear();

            foreach (var item in matchedVacations)
            {
                showVacations.Children.Add(new VacationItem(this, item));
            }
        }

        private void OpenVacationRequest(object sender, RoutedEventArgs e)
        {
            var emp = employeeContext.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);

            if (!CanRequestVacation(emp))
            {
                MessageBox.Show("Вы можете подать заявление на отпуск только через 6 месяцев после трудоустройства.", "Отказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainWindow.mw.OpenPages(new VacationRequest());
        }

        private bool CanRequestVacation(Models.Employees employee)
        {
            return DateTime.Now >= employee.HireDate.AddMonths(6);
        }

        private bool showingPendingOnly = false;

        private void ShowPendingRequests(object sender, RoutedEventArgs e)
        {
            if (showingPendingOnly)
            {
                LoadVacations();
                showingPendingOnly = false;
            }
            else
            {
                var pendingVacations = VacationContext.Vacations.Where(v => v.StatusId == 1).ToList();

                showVacations.Children.Clear();

                foreach (var item in pendingVacations)
                {
                    showVacations.Children.Add(new VacationItem(this, item));
                }

                showingPendingOnly = true;
            }
        }

        private void ExportVacations(object sender, RoutedEventArgs e)
        {
            try
            {
                var dateDialog = new DateRangeDialog();
                if (dateDialog.ShowDialog() != true)
                    return;

                DateTime startDate = dateDialog.StartDate.Value;
                DateTime endDate = dateDialog.EndDate.Value;
                string format = dateDialog.SelectedFormat;

                var vacations = VacationContext.Vacations
                    .Where(v => v.StartDate <= endDate && v.EndDate >= startDate)
                    .OrderBy(v => v.StartDate)
                    .ToList();

                if (vacations.Count == 0)
                {
                    MessageBox.Show("Нет данных для экспорта в выбранном диапазоне.", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var defaultFileName = $"Vacations_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

                var saveDialog = new Microsoft.Win32.SaveFileDialog();

                if (format == "PDF")
                {
                    saveDialog.FileName = defaultFileName;
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                }
                else if (format == "Excel")
                {
                    saveDialog.FileName = defaultFileName;
                    saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                }

                bool? saveResult = saveDialog.ShowDialog();
                if (saveResult != true)
                    return;

                string filePath = saveDialog.FileName;

                if (format == "PDF")
                    ExportToPdf(vacations, filePath, startDate, endDate);
                else if (format == "Excel")
                    ExportToExcel(vacations, filePath, startDate, endDate);

                MessageBox.Show("Экспорт выполнен успешно!", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel(List<Models.Vacations> vacations, string filePath, DateTime startDate, DateTime endDate)
        {
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Отпуска");

            ws.Cell(1, 1).Value = $"Отпуска за период: {startDate:dd.MM.yyyy} — {endDate:dd.MM.yyyy}";
            ws.Range(1, 1, 1, 6).Merge().Style.Font.Bold = true;
            ws.Range(1, 1, 1, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(2, 1).Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Range(2, 1, 2, 6).Merge().Style.Font.Italic = true;
            ws.Range(2, 1, 2, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(3, 1).Value = "Сотрудник";
            ws.Cell(3, 2).Value = "Дата начала";
            ws.Cell(3, 3).Value = "Дата окончания";
            ws.Cell(3, 4).Value = "Кол-во дней";
            ws.Cell(3, 5).Value = "Тип отпуска";
            ws.Cell(3, 6).Value = "Статус";

            for (int i = 0; i < vacations.Count; i++)
            {
                var vac = vacations[i];
                var employee = employeeContext.Employees.FirstOrDefault(e => e.Id == vac.EmployeeId);
                var type = typeContext.VacationTypes.FirstOrDefault(t => t.Id == vac.TypeId);
                var status = statusContext.VacationStatus.FirstOrDefault(s => s.Id == vac.StatusId);

                ws.Cell(i + 4, 1).Value = employee != null ? $"{employee.LastName} {employee.FirstName} {employee.Patronymic}" : "Не найден";
                ws.Cell(i + 4, 2).Value = vac.StartDate.ToString("dd.MM.yyyy");
                ws.Cell(i + 4, 3).Value = vac.EndDate.ToString("dd.MM.yyyy");
                ws.Cell(i + 4, 4).Value = (vac.EndDate - vac.StartDate).Days + 1;
                ws.Cell(i + 4, 5).Value = type?.Name ?? "";
                ws.Cell(i + 4, 6).Value = status?.Name ?? "";
            }

            ws.Columns().AdjustToContents();

            var lastRow = vacations.Count + 3;
            var dataRange = ws.Range(1, 1, lastRow, 6);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            wb.SaveAs(filePath);
        }

        private void ExportToPdf(List<Models.Vacations> vacations, string filePath, DateTime periodStart, DateTime periodEnd)
        {
            var document = new MigraDocDocument();
            var section = document.AddSection();

            var title = section.AddParagraph("Список отпусков");
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Center;
            title.Format.SpaceAfter = "0.5cm";

            var periodInfo = section.AddParagraph($"Отпуска за период: {periodStart:dd.MM.yyyy} по {periodEnd:dd.MM.yyyy}");
            periodInfo.Format.Font.Size = 12;
            periodInfo.Format.Font.Italic = true;
            periodInfo.Format.SpaceAfter = "1cm";
            periodInfo.Format.Alignment = ParagraphAlignment.Center;

            var table = section.AddTable();
            table.Borders.Width = 0.75;

            table.AddColumn(Unit.FromCentimeter(4));
            table.AddColumn(Unit.FromCentimeter(3));
            table.AddColumn(Unit.FromCentimeter(3));
            table.AddColumn(Unit.FromCentimeter(2.5));
            table.AddColumn(Unit.FromCentimeter(3));
            table.AddColumn(Unit.FromCentimeter(3));

            var headerRow = table.AddRow();
            headerRow.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGray;
            headerRow.Cells[0].AddParagraph("Сотрудник");
            headerRow.Cells[1].AddParagraph("Начало отпуска");
            headerRow.Cells[2].AddParagraph("Конец отпуска");
            headerRow.Cells[3].AddParagraph("Кол-во дней");
            headerRow.Cells[4].AddParagraph("Тип");
            headerRow.Cells[5].AddParagraph("Статус");

            foreach (var vac in vacations)
            {
                var employee = employeeContext.Employees.FirstOrDefault(x => x.Id == vac.EmployeeId);
                var typeName = typeContext.VacationTypes.FirstOrDefault(x => x.Id == vac.TypeId)?.Name ?? "";
                var statusName = statusContext.VacationStatus.FirstOrDefault(x => x.Id == vac.StatusId)?.Name ?? "";

                var row = table.AddRow();
                row.Cells[0].AddParagraph($"{employee?.LastName} {employee?.FirstName} {employee?.Patronymic}");
                row.Cells[1].AddParagraph(vac.StartDate.ToString("dd.MM.yyyy"));
                row.Cells[2].AddParagraph(vac.EndDate.ToString("dd.MM.yyyy"));
                row.Cells[3].AddParagraph($"{(vac.EndDate - vac.StartDate).Days + 1}");
                row.Cells[4].AddParagraph(typeName);
                row.Cells[5].AddParagraph(statusName);
            }

            var footer = section.AddParagraph();
            footer.Format.SpaceBefore = "1cm";
            footer.Format.Alignment = ParagraphAlignment.Right;
            footer.AddFormattedText($"Дата формирования документа: {DateTime.Now:dd.MM.yyyy}", TextFormat.Italic);

            var pdfRenderer = new PdfDocumentRenderer(true) { Document = document };
            pdfRenderer.RenderDocument();

            pdfRenderer.PdfDocument.Save(filePath);
        }
    }
}
