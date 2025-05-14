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
    /// Логика взаимодействия для VacationEdit.xaml
    /// </summary>
    public partial class VacationEdit : Page
    {
        Vacations MainVacations;
        Models.Vacations vacations;

        EmployeeContext employeeContext = new EmployeeContext();
        VacationContext vacationContext = new VacationContext();
        VacationTypeContext typeContext = new VacationTypeContext();
        VacationStatusContext statusContext = new VacationStatusContext();

        public VacationEdit(Vacations MainVacations, Models.Vacations vacations)
        {
            InitializeComponent();

            this.MainVacations = MainVacations;
            this.vacations = vacations;

            foreach (var item in employeeContext.Employees)
                Employee.Items.Add(item.LastName + " " + item.FirstName + " " + item.Patronymic);

            foreach (var item in typeContext.VacationTypes)
                Type.Items.Add(item.Name);

            foreach (var item in statusContext.VacationStatus)
                Status.Items.Add(item.Name);

            if (vacations != null)
            {
                lbTitle.Content = "Изменение существующего отпуска";

                var selectedEmployee = employeeContext.Employees.FirstOrDefault(x => x.Id == vacations.EmployeeId);
                if (selectedEmployee != null)
                {
                    Employee.SelectedItem = selectedEmployee.LastName + " " + selectedEmployee.FirstName + " " + selectedEmployee.Patronymic;
                }

                dateStart.SelectedDate = vacations.StartDate;
                dateEnd.SelectedDate = vacations.EndDate;
                Type.SelectedItem = typeContext.VacationTypes.Where(x => x.Id == vacations.TypeId).FirstOrDefault()?.Name;
                Status.SelectedItem = statusContext.VacationStatus.Where(x => x.Id == vacations.StatusId).FirstOrDefault()?.Name;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dateStart.SelectedDate == null || dateEnd.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите дату начала и дату окончания отпуска.");
                    return;
                }

                DateTime start = dateStart.SelectedDate.Value;
                DateTime end = dateEnd.SelectedDate.Value;
                if (end < start)
                {
                    MessageBox.Show("Дата окончания отпуска не может быть раньше даты начала.");
                    return;
                }
                if (Employee.SelectedItem == null)
                {
                    MessageBox.Show("Выберите сотрудника.");
                    return;
                }

                var employeeFullName = Employee.SelectedItem.ToString();
                var employee = employeeContext.Employees.FirstOrDefault(x =>
                    (x.LastName + " " + x.FirstName + " " + x.Patronymic) == employeeFullName);
                if (employee == null)
                {
                    MessageBox.Show("Не удалось найти сотрудника. Проверьте правильность выбора.");
                    return;
                }
                if (Type.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип отпуска.");
                    return;
                }

                var vacationType = typeContext.VacationTypes.FirstOrDefault(x => x.Name == Type.SelectedItem.ToString());
                if (vacationType == null)
                {
                    MessageBox.Show("Тип отпуска не найден.");
                    return;
                }

                if (Status.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус отпуска.");
                    return;
                }

                var vacationStatus = statusContext.VacationStatus.FirstOrDefault(x => x.Name == Status.SelectedItem.ToString());
                if (vacationStatus == null)
                {
                    MessageBox.Show("Статус отпуска не найден.");
                    return;
                }

                if (vacations == null)
                {
                    vacations = new Models.Vacations()
                    {
                        EmployeeId = employee.Id,
                        StartDate = start,
                        EndDate = end,
                        TypeId = vacationType.Id,
                        StatusId = vacationStatus.Id
                    };
                    MainVacations.VacationContext.Vacations.Add(vacations);
                }
                else
                {
                    vacations.EmployeeId = employee.Id;
                    vacations.StartDate = start;
                    vacations.EndDate = end;
                    vacations.TypeId = vacationType.Id;
                    vacations.StatusId = vacationStatus.Id;
                }
                MainVacations.VacationContext.SaveChanges();
                MessageBox.Show("Отпуск успешно сохранён.");

                MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка при сохранении отпуска.\n" + ex.Message);
            }
        }
    }
}
