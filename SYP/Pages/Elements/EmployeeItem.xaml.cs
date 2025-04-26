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

        public EmployeeItem(Employees MainEmployees, Models.Employees Employee)
        {
            InitializeComponent();

            this.MainEmployees = MainEmployees;
            this.Employee = Employee;

            lbFIO.Content = Employee.LastName + " " + Employee.FirstName + " " + Employee.Patronymic;
            lbPosition.Content = "Должность: " + positionContext.Positions.Where(x => x.Id == Employee.PositionId).FirstOrDefault().Name;
            lbDepartment.Content = "Отдел: " + departmentContext.Departments.Where(x => x.Id == Employee.DepartmentId).FirstOrDefault().Name;
            lbStatus.Content = Employee.Status;

            if (Employee.Status == "Активен")
            {
                lbStatus.Foreground = (Brush)new BrushConverter().ConvertFrom("#239417");
            }
            else if (Employee.Status == "В отпуске")
            {
                lbStatus.Foreground = (Brush)new BrushConverter().ConvertFrom("#BF9115");
            }
            else if (Employee.Status == "Уволен")
            {
                lbStatus.Foreground = (Brush)new BrushConverter().ConvertFrom("#A31717");
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
