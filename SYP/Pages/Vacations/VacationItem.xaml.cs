using SYP.Context;
using SYP.Models;
using SYP.Pages.Positions;
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
    /// Логика взаимодействия для VacationItem.xaml
    /// </summary>
    public partial class VacationItem : UserControl
    {
        Vacations MainVacations;
        Models.Vacations Vacation;
        EmployeeContext employeeContext = new EmployeeContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        VacationStatusContext statusContext = new VacationStatusContext();
        private Models.Users currentUser;

        public VacationItem(Vacations MainVacations, Models.Vacations Vacation)
        {
            InitializeComponent();

            this.MainVacations = MainVacations;
            this.Vacation = Vacation;

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                Edit.Visibility = Visibility.Visible;
                Delete.Visibility = Visibility.Visible;
            }

            var employee = employeeContext.Employees.FirstOrDefault(x => x.Id == Vacation.EmployeeId);
            lbEmployee.Content = "Сотрудник: " + employee.LastName + " " + employee.FirstName + " " + employee.Patronymic;
            lbStartDate.Content = "Начало отпуска: " + Vacation.StartDate.ToString("dd.MM.yyyy");
            lbEndDate.Content = "Конец отпуска: " + Vacation.EndDate.ToString("dd.MM.yyyy");
            lbType.Content = "Тип: " + typeContext.VacationTypes.FirstOrDefault(x => x.Id == Vacation.TypeId).Name;
            lbStatus.Content = "Статус: " + statusContext.VacationStatus.FirstOrDefault(x => x.Id == Vacation.StatusId).Name;

            if (Vacation.StatusId == 4)
            {
                var lightGray = (Color)ColorConverter.ConvertFromString("#FFEEEEEE");
                border.Background = new SolidColorBrush(lightGray);
                Edit.Visibility = Visibility.Collapsed;
                Delete.Visibility = Visibility.Collapsed;
            }
            else
            {
                Edit.Visibility = Visibility.Visible;
                Delete.Visibility = Visibility.Visible;
            }
        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new VacationEdit(MainVacations, Vacation));
        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить отпуск?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new VacationContext())
                    {
                        var vacationToDelete = context.Vacations.FirstOrDefault(x => x.Id == Vacation.Id);
                        if (vacationToDelete != null)
                        {
                            context.Vacations.Remove(vacationToDelete);
                            context.SaveChanges();
                            MessageBox.Show("Отпуск удалён");
                            (this.Parent as Panel).Children.Remove(this);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка.\n" + ex.Message);
                }
            }
        }
    }
}
