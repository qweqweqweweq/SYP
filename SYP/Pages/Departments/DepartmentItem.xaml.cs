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
    /// Логика взаимодействия для DepartmentItem.xaml
    /// </summary>
    public partial class DepartmentItem : UserControl
    {
        Departments MainDepartments;
        Models.Departments Department;

        public DepartmentItem(Departments MainDepartments, Models.Departments Department)
        {
            InitializeComponent();

            this.MainDepartments = MainDepartments;
            this.Department = Department;

            lbName.Content = Department.Name;
        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
