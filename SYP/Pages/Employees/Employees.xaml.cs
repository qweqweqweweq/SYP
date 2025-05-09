using SYP.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для Employees.xaml
    /// </summary>
    public partial class Employees : Page
    {
        public EmployeeContext EmployeeContext = new EmployeeContext();
        public StatusContext StatusContext = new StatusContext();
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
            }
            UpdateEmployeeStatuses();
            LoadEmployees();

            foreach (var item in StatusContext.Status) Status.Items.Add(item.Name);
        }

        private void LoadEmployees()
        {
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

        private void OpenReports(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Reports());
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Settings());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Authorization());
        }

        private void KeyDownSearch(object sender, KeyEventArgs e)
        {

        }

        private void SelectedStatus(object sender, SelectionChangedEventArgs e)
        {
            
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
    }
}
