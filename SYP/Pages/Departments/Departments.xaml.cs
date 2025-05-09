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

namespace SYP.Pages.Departments
{
    /// <summary>
    /// Логика взаимодействия для Departments.xaml
    /// </summary>
    public partial class Departments : Page
    {
        public DepartmentContext DepartmentContext = new DepartmentContext();
        private Models.Users currentUser;

        public Departments()
        {
            InitializeComponent();

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                add.Visibility = Visibility.Visible;
            }

            showDepartments.Children.Clear();
            foreach (Models.Departments item in DepartmentContext.Departments)
                showDepartments.Children.Add(new Pages.Departments.DepartmentItem(this, item));
        }

        private void OpenMain(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Main());
        }

        private void OpenEmployees(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees.Employees());
        }

        private void OpenPositions(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Positions.Positions());
        }

        private void OpenVacations(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
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

        private void KeyDownSearch(object sender, KeyEventArgs e)
        {

        }

        private void AddDepartment(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Departments.DepartmentEdit(this, null));
        }

        private void SearchDepartment(object sender, TextChangedEventArgs e)
        {
            string searchText = search.Text.ToLower();
            var result = DepartmentContext.Departments.Where(x => x.Name.ToLower().Contains(searchText));
            showDepartments.Children.Clear();
            foreach (var item in result)
            {
                showDepartments.Children.Add(new DepartmentItem(this, item));
            }
        }
    }
}
