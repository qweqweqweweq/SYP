using SYP.Context;
using SYP.Models;
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
    /// Логика взаимодействия для VacationRequest.xaml
    /// </summary>
    public partial class VacationRequest : Page
    {
        VacationContext vacationContext = new VacationContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        EmployeeContext employeeContext = new EmployeeContext();
        Models.Vacations vacations;
        private Users currentUser;

        public VacationRequest()
        {
            InitializeComponent();

            currentUser = MainWindow.mw.CurrentUser;

            if (currentUser == null || currentUser.EmployeeId == null)
            {
                MessageBox.Show("Ошибка: невозможно определить текущего сотрудника.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var emp = employeeContext.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);
            if (emp != null)
                Employee.Content = $"{emp.LastName} {emp.FirstName} {emp.Patronymic}";

            foreach (var type in typeContext.VacationTypes)
                Type.Items.Add(type.Name);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Vacations());
        }

        private void SendRequest(object sender, RoutedEventArgs e)
        {
            if (dateStart.SelectedDate == null || dateEnd.SelectedDate == null || Type.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (dateStart.SelectedDate > dateEnd.SelectedDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedType = Type.SelectedItem as VacationTypes;
            if (selectedType == null)
            {
                MessageBox.Show("Выбран некорректный тип отпуска.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newVacation = new Models.Vacations
            {
                EmployeeId = currentUser.EmployeeId,
                StartDate = dateStart.SelectedDate.Value,
                EndDate = dateEnd.SelectedDate.Value,
                TypeId = selectedType.Id,
                StatusId = 1
            };

            try
            {
                vacationContext.Vacations.Add(newVacation);
                vacationContext.SaveChanges();

                MessageBox.Show("Заявка успешно отправлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.mw.OpenPages(new Vacations());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: \n" + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
