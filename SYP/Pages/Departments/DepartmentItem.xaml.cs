using SYP.Context;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYP.Pages.Departments
{
    /// <summary>
    /// Логика взаимодействия для DepartmentItem.xaml
    /// </summary>
    public partial class DepartmentItem : UserControl
    {
        Departments MainDepartments;
        Models.Departments Department;
        private Models.Users currentUser;

        public DepartmentItem(Departments MainDepartments, Models.Departments Department)
        {
            InitializeComponent();

            this.MainDepartments = MainDepartments;
            this.Department = Department;

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                Edit.Visibility = Visibility.Visible;
                Delete.Visibility = Visibility.Visible;
            }

            lbName.Content = Department.Name;
        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Departments.DepartmentEdit(MainDepartments, Department));
        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить отдел?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var context = new DepartmentContext())
                {
                    var departmentToDelete = context.Departments.FirstOrDefault(x => x.Id == Department.Id);
                    if (departmentToDelete != null)
                    {
                        context.Departments.Remove(departmentToDelete);
                        context.SaveChanges();
                        MessageBox.Show("Отдел удалён");
                        (this.Parent as Panel).Children.Remove(this);
                    }
                }
            }
        }

        private void OpenEmployeesDept(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees.Employees(Department));
        }
    }
}
