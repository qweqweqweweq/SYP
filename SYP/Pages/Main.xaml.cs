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

        public Main()
        {
            InitializeComponent();

            CountDepartments.Content = "Всего отделов: " + DepartmentContext.Departments.Count();
            CountEmployees.Content = "Всего сотрудников: " + EmployeeContext.Employees.Count();

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

        private void OpenEmployees(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees());
        }

        private void OpenDepartments(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Departments());
        }

        private void OpenPositions(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Positions());
        }

        private void OpenVacations(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations());
        }

        private void OpenReports(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Reports());
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Settings());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Authorization());
        }
    }
}
