using SYP.Context;
using SYP.Models;
using SYP.Pages.Employees;
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

            var vacations = VacationContext.Vacations
                .OrderBy(v => v.StatusId == 4)
                .ThenByDescending(v => v.StartDate)
                .ToList();

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
            MainWindow.mw.OpenPages(new VacationRequest());
        }

        private void ShowPendingRequests(object sender, RoutedEventArgs e)
        {
            var pendingVacations = VacationContext.Vacations.Where(v => v.StatusId == 1).ToList();

            showVacations.Children.Clear();

            foreach (var item in pendingVacations)
            {
                showVacations.Children.Add(new VacationItem(this, item));
            }
        }
    }
}
