using SYP.Context;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для Employees.xaml
    /// </summary>
    public partial class Employees : Page
    {
        public EmployeeContext EmployeeContext = new EmployeeContext();
        public EmployeeStatusContext StatusContext = new EmployeeStatusContext();
        public DepartmentContext DepartmentContext = new DepartmentContext();
        public PositionContext PositionContext = new PositionContext();
        VacationContext VacationContext = new VacationContext();
        private Models.Users currentUser;

        public Employees()
        {
            InitializeComponent();

            ProfileOverlay.MainEmployees = this;

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                add.Visibility = Visibility.Visible;
                settings.Visibility = Visibility.Hidden;
            }
            UpdateEmployeeStatuses();
            LoadEmployees();

            foreach (var item in DepartmentContext.Departments) Department.Items.Add(item.Name);
            foreach (var item in PositionContext.Positions) Position.Items.Add(item.Name);
            foreach (var item in StatusContext.Status) Status.Items.Add(item.Name);
        }

        private void LoadEmployees()
        {
            if (showEmployees == null) return;

            showEmployees.Children.Clear();
            foreach (Models.Employees item in EmployeeContext.Employees)
                showEmployees.Children.Add(new EmployeeItem(this, item));
        }

        private void UpdateEmployeeStatuses()
        {
            var today = DateTime.Today;
            int activeId = StatusContext.Status.First(s => s.Name == "Активен").Id;
            int onVacationId = StatusContext.Status.First(s => s.Name == "В отпуске").Id;

            foreach (var emp in EmployeeContext.Employees)
            {
                bool inVac = VacationContext.Vacations
                    .Any(v => v.EmployeeId == emp.Id
                           && v.StartDate <= today
                           && v.EndDate >= today);
                if (inVac && emp.StatusId != onVacationId)
                {
                    emp.StatusId = onVacationId;
                }
                else if (!inVac)
                {
                    var lastVac = VacationContext.Vacations
                        .Where(v => v.EmployeeId == emp.Id)
                        .OrderByDescending(v => v.EndDate)
                        .FirstOrDefault();
                    if (lastVac != null && today > lastVac.EndDate && emp.StatusId != activeId)
                    {
                        emp.StatusId = activeId;
                    }
                }
            }
            EmployeeContext.SaveChanges();
        }

        private void OpenMain(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Main());
        }

        private void OpenDepartments(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Departments.Departments());
        }

        private void OpenPositions(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Positions.Positions());
        }

        private void OpenVacations(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Vacations.Vacations());
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Settings());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Authorization.Authorization());
        }

        private void SelectedStatus(object sender, SelectionChangedEventArgs e)
        {
            if (Status.SelectedIndex <= 0)
            {
                LoadEmployees();
                return;
            }

            string selectedStatusName = Status.SelectedItem.ToString();
            var selectedStatus = StatusContext.Status.FirstOrDefault(s => s.Name == selectedStatusName);

            if (selectedStatus != null)
            {
                var matchedEmployees = EmployeeContext.Employees.Where(e => e.StatusId == selectedStatus.Id).ToList();

                showEmployees.Children.Clear();

                foreach (var item in matchedEmployees)
                {
                    showEmployees.Children.Add(new EmployeeItem(this, item));
                }
            }
        }

        private void AddEmployee(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees.EmployeeEdit(this, null));
        }

        private void SearchEmployee(object sender, TextChangedEventArgs e)
        {
            string searchText = search.Text.ToLower();

            var allEmployees = EmployeeContext.Employees.ToList();
            var result = allEmployees.Where(x => $"{x.LastName} {x.FirstName} {x.Patronymic}".ToLower().Contains(searchText)).ToList();

            showEmployees.Children.Clear();
            foreach (var item in result)
            {
                showEmployees.Children.Add(new EmployeeItem(this, item));
            }
        }

        private void SelectedDepartment(object sender, SelectionChangedEventArgs e)
        {
            if (Department.SelectedIndex <= 0)
            {
                LoadEmployees();
                return;
            }

            string selectedDepartmentName = Department.SelectedItem.ToString();
            var selectedDepartment = DepartmentContext.Departments.FirstOrDefault(s => s.Name == selectedDepartmentName);

            if (selectedDepartment != null)
            {
                var matchedEmployees = EmployeeContext.Employees.Where(e => e.DepartmentId == selectedDepartment.Id).ToList();

                showEmployees.Children.Clear();

                foreach (var item in matchedEmployees)
                {
                    showEmployees.Children.Add(new EmployeeItem(this, item));
                }
            }
        }

        private void SelectedPosition(object sender, SelectionChangedEventArgs e)
        {
            if (Position.SelectedIndex <= 0)
            {
                LoadEmployees();
                return;
            }

            string selectedPositionName = Position.SelectedItem.ToString();
            var selectedPosition = PositionContext.Positions.FirstOrDefault(s => s.Name == selectedPositionName);

            if (selectedPosition != null)
            {
                var matchedEmployees = EmployeeContext.Employees.Where(e => e.PositionId == selectedPosition.Id).ToList();

                showEmployees.Children.Clear();

                foreach (var item in matchedEmployees)
                {
                    showEmployees.Children.Add(new EmployeeItem(this, item));
                }
            }
        }
    }
}
