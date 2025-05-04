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

namespace SYP.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для VacationItem.xaml
    /// </summary>
    public partial class VacationItem : UserControl
    {
        Vacations MainVacations;
        Models.Vacations Vacation;
        EmployeeContext employeeContext = new EmployeeContext();

        public VacationItem(Vacations MainVacations, Models.Vacations Vacation)
        {
            InitializeComponent();

            this.MainVacations = MainVacations;
            this.Vacation = Vacation;

            var employee = employeeContext.Employees.FirstOrDefault(x => x.Id == Vacation.EmployeeId);
            lbEmployee.Content = "Сотрудник: " + employee.LastName + " " + employee.FirstName + " " + employee.Patronymic;
            lbStartDate.Content = "Начало отпуска: " + Vacation.StartDate;
            lbEndDate.Content = "Конец отпуска: " + Vacation.EndDate;
            lbType.Content = "Тип: " + Vacation.Type;
        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
