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
        VacationTypeContext typeContext = new VacationTypeContext();

        public VacationEdit(Vacations MainVacations, Models.Vacations vacations)
        {
            InitializeComponent();

            this.MainVacations = MainVacations;
            this.vacations = vacations;

            foreach (var item in employeeContext.Employees)
                Employee.Items.Add(item.LastName + " " + item.FirstName + " " + item.Patronymic);

            foreach (var item in typeContext.VacationTypes)
                Type.Items.Add(item.Name);

            if (vacations != null)
            {
                lbTitle.Content = "Изменение существующего отпуска";

                Employee.SelectedItem = employeeContext.Employees.Where(x => x.Id == vacations.EmployeeId).FirstOrDefault()?.LastName + " " + 
                    employeeContext.Employees.Where(x => x.Id == vacations.EmployeeId).FirstOrDefault()?.FirstName;
                dateStart.SelectedDate = vacations.StartDate;
                dateEnd.SelectedDate = vacations.EndDate;
                Type.SelectedItem = typeContext.VacationTypes.Where(x => x.Id == vacations.TypeId).FirstOrDefault()?.Name;
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
                DateTime start = dateStart.SelectedDate.Value;
                DateTime end = dateEnd.SelectedDate.Value;
                var employee = employeeContext.Employees.Where(x => x.LastName + " " + x.FirstName + " " + x.Patronymic == Employee.SelectedItem.ToString()).FirstOrDefault();
                var vacationType = typeContext.VacationTypes.Where(x => x.Name == Type.SelectedItem.ToString()).FirstOrDefault();

                if (Employee.SelectedItem == null)
                {
                    MessageBox.Show("Выберите сотрудника.");
                    return;
                }
                if (dateStart.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату начала отпуска.");
                    return;
                }
                if (dateEnd.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату окончания отпуска.");
                    return;
                }
                if (end < start)
                {
                    MessageBox.Show("Дата окончания отпуска не может быть раньше даты начала.");
                    return;
                }
                if (Type.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип отпуска.");
                    return;
                }

                if (vacations == null)
                {
                    vacations = new Models.Vacations()
                    {
                        EmployeeId = employee.Id,
                        StartDate = start,
                        EndDate = end,
                        TypeId = vacationType.Id
                    };
                    MainVacations.VacationContext.Vacations.Add(vacations);
                }
                else
                {
                    vacations.EmployeeId = employee.Id;
                    vacations.StartDate = start;
                    vacations.EndDate = end;
                    vacations.TypeId = vacationType.Id;
                }
                MainVacations.VacationContext.SaveChanges();
                MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка.\n" + ex.Message);
            }
        }
    }
}
