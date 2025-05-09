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

        }
    }
}
