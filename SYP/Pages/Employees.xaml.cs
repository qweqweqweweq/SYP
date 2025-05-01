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
    /// Логика взаимодействия для Employees.xaml
    /// </summary>
    public partial class Employees : Page
    {
        public EmployeeContext EmployeeContext = new EmployeeContext();
        public StatusContext StatusContext = new StatusContext();

        public Employees()
        {
            InitializeComponent();

            showEmployees.Children.Clear();
            foreach (Models.Employees item in EmployeeContext.Employees)
                showEmployees.Children.Add(new Elements.EmployeeItem(this, item));

            foreach (var item in StatusContext.Status) Status.Items.Add(item.Name);
        }

        private void OpenMain(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Main());
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

        private void KeyDownSearch(object sender, KeyEventArgs e)
        {

        }

        private void SelectedStatus(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}
