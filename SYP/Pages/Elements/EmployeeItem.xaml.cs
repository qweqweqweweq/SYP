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

namespace SYP.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для EmployeeItem.xaml
    /// </summary>
    public partial class EmployeeItem : UserControl
    {
        Employees MainEmployees;
        Models.Employees Employee;
        PositionContext positionContext = new PositionContext();
        DepartmentContext departmentContext = new DepartmentContext();
        StatusContext statusContext = new StatusContext();

        public EmployeeItem(Employees MainEmployees, Models.Employees Employee)
        {
            InitializeComponent();

            this.MainEmployees = MainEmployees;
            this.Employee = Employee;

            lbFIO.Content = Employee.LastName + " " + Employee.FirstName + " " + Employee.Patronymic;
            lbPosition.Content = "Должность: " + positionContext.Positions.Where(x => x.Id == Employee.PositionId).FirstOrDefault().Name;
            lbDepartment.Content = "Отдел: " + departmentContext.Departments.Where(x => x.Id == Employee.DepartmentId).FirstOrDefault().Name;

            var status = statusContext.Status.FirstOrDefault(x => x.Id == Employee.StatusId);
            lbStatus.Content = status.Name;

            if (status.Name == "Активен")
            {
                lbStatus.Foreground = new BrushConverter().ConvertFrom("#258117") as Brush;
            }
            else if (status.Name == "В отпуске")
            {
                lbStatus.Foreground = new BrushConverter().ConvertFrom("#fc7e01") as Brush;
            }
            else if (status.Name == "Уволен")
            {
                lbStatus.Foreground = new BrushConverter().ConvertFrom("#ac2517") as Brush;
            }
        }

        private void ViewClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
