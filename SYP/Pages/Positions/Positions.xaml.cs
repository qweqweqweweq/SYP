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

namespace SYP.Pages.Positions
{
    /// <summary>
    /// Логика взаимодействия для Positions.xaml
    /// </summary>
    public partial class Positions : Page
    {
        public PositionContext PositionContext = new PositionContext();
        private Models.Users currentUser;

        public Positions()
        {
            InitializeComponent();

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                add.Visibility = Visibility.Visible;
                settings.Visibility = Visibility.Hidden;
            }

            showPositions.Children.Clear();
            foreach (Models.Positions item in PositionContext.Positions)
                showPositions.Children.Add(new Pages.Positions.PositionItem(this, item));
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

        private void OpenVacations(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Settings());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Authorization.Authorization());
        }

        private void KeyDownSearch(object sender, KeyEventArgs e)
        {

        }

        private void AddPosition(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Positions.PositionEdit(this, null));
        }

        private void SearchPosition(object sender, TextChangedEventArgs e)
        {
            string searchText = search.Text.ToLower();
            var result = PositionContext.Positions.Where(x => x.Name.ToLower().Contains(searchText));
            showPositions.Children.Clear();
            foreach (var item in result)
            {
                showPositions.Children.Add(new PositionItem(this, item));
            }
        }
    }
}
