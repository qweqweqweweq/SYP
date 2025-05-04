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

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        Employees MainEmployees;
        Models.Employees employees;
        DepartmentContext departmentContext = new DepartmentContext();
        StatusContext statusContext = new StatusContext();
        PositionContext positionContext = new PositionContext();

        public Profile(Employees mainEmployees, Models.Employees employees)
        {
            InitializeComponent();

            this.MainEmployees = mainEmployees;
            this.employees = employees;

            fio.Content = "ФИО: " + employees.LastName + " " + employees.FirstName + " " + employees.Patronymic;
            dateBirth.Content = "Дата рождения: " + employees.BirthDate;
            department.Content = "Отдел: " + departmentContext.Departments.FirstOrDefault(x => x.Id == employees.DepartmentId).Name;
            position.Content = "Должность: " + positionContext.Positions.FirstOrDefault(x => x.Id == employees.PositionId).Name;
            dateHire.Content = "Дата поступления: " + employees.HireDate;
            email.Content = "Эл. почта: " + employees.Email;
            phone.Content = "Номер телефона: " + employees.PhoneNumber;
            status.Content = "Статус: " + statusContext.Status.FirstOrDefault(x => x.Id == employees.StatusId).Name;
        }

        private void Close(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees.Employees());
        }
    }
}
