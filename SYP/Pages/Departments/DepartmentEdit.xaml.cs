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

namespace SYP.Pages.Departments
{
    /// <summary>
    /// Логика взаимодействия для DepartmentEdit.xaml
    /// </summary>
    public partial class DepartmentEdit : Page
    {
        Departments MainDepartments;
        Models.Departments departments;

        public DepartmentEdit(Departments MainDepartments, Models.Departments departments)
        {
            InitializeComponent();

            this.MainDepartments = MainDepartments;
            this.departments = departments;

            if (departments != null)
            {
                lbTitle.Content = "Изменение существующего отдела";
                NameDepartment.Text = departments.Name;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Departments());
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isNameValid = !string.IsNullOrWhiteSpace(NameDepartment.Text);

                RegexValidator.ValidateControl(NameDepartment, isNameValid);

                if (!isNameValid)
                {
                    MessageBox.Show("Введите наименование отдела.");
                    return;
                }

                if (departments == null)
                {
                    departments = new Models.Departments()
                    {
                        Name = NameDepartment.Text
                    };
                    MainDepartments.DepartmentContext.Departments.Add(departments);
                }
                else
                {
                    departments.Name = NameDepartment.Text;
                }
                MainDepartments.DepartmentContext.SaveChanges();
                MainWindow.mw.OpenPages(new Pages.Departments.Departments());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка.\n" + ex.Message);
            }
        }
    }
}
