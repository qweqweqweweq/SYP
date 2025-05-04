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

namespace SYP.Pages.Employees
{
    /// <summary>
    /// Логика взаимодействия для Employees.xaml
    /// </summary>
    public partial class EmployeeEdit : Page
    {
        Employees MainEmployees;
        Models.Employees employees;

        DepartmentContext departmentContext = new DepartmentContext();
        PositionContext positionContext = new PositionContext();
        StatusContext statusContext = new StatusContext();

        public EmployeeEdit(Employees MainEmployees, Models.Employees employees)
        {
            InitializeComponent();

            this.MainEmployees = MainEmployees;
            this.employees = employees;

            foreach (var item in positionContext.Positions)
                position.Items.Add(item.Name);

            foreach (var item in departmentContext.Departments)
                department.Items.Add(item.Name);

            foreach (var item in statusContext.Status)
                status.Items.Add(item.Name);

            if (employees != null)
            {
                lbTitle.Content = "Изменение существующего сотрудника";
                FIO.Text = employees.LastName + " " + employees.FirstName + " " + employees.Patronymic;
                dateBirth.Text = employees.BirthDate.ToString("dd.MM.yyyy");
                position.SelectedItem = positionContext.Positions.Where(x => x.Id == employees.PositionId).FirstOrDefault().Name;
                department.SelectedItem = departmentContext.Departments.Where(x => x.Id == employees.DepartmentId).FirstOrDefault().Name;
                dateHire.Text = employees.HireDate.ToString("dd.MM.yyyy");
                email.Text = employees.Email;
                phoneNumber.Text = employees.PhoneNumber;
                status.SelectedItem = statusContext.Status.Where(x => x.Id == employees.StatusId).FirstOrDefault().Name;
            }

            CheckValidation();
        }

        private void CheckValidation()
        {
            FIO.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(FIO, RegexValidator.IsFullNameValid(FIO.Text));

            email.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(email, RegexValidator.IsEmailValid(email.Text));

            phoneNumber.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(phoneNumber, RegexValidator.IsPhoneValid(phoneNumber.Text));

            position.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(position, position.SelectedItem != null);

            department.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(department, department.SelectedItem != null);

            status.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(status, status.SelectedItem != null);

            dateBirth.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(dateBirth, dateBirth.SelectedDate != null);

            dateHire.LostFocus += (s, e) =>
            RegexValidator.ValidateControl(dateHire, dateHire.SelectedDate != null);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Employees());
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!RegexValidator.IsFullNameValid(FIO.Text))
                {
                    MessageBox.Show("Введите корректное ФИО (только буквы, минимум 5 символов).");
                    return;
                }

                if (!RegexValidator.IsEmailValid(email.Text))
                {
                    MessageBox.Show("Введите корректный Email.");
                    return;
                }

                if (!RegexValidator.IsPhoneValid(phoneNumber.Text))
                {
                    MessageBox.Show("Введите корректный номер телефона (формат: +7XXXXXXXXXX).");
                    return;
                }

                if (dateBirth.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату рождения.");
                    return;
                }

                if (dateHire.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату поступления.");
                    return;
                }

                if (position.SelectedItem == null)
                {
                    MessageBox.Show("Выберите должность.");
                    return;
                }

                if (department.SelectedItem == null)
                {
                    MessageBox.Show("Выберите отдел.");
                    return;
                }

                if (status.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус.");
                    return;
                }

                string[] fullNameParts = FIO.Text.Trim().Split(' ');

                string lastName = fullNameParts.Length > 0 ? fullNameParts[0] : "";
                string firstName = fullNameParts.Length > 1 ? fullNameParts[1] : "";
                string patronymic = fullNameParts.Length > 2 ? fullNameParts[2] : "";

                DateTime birth = dateBirth.SelectedDate.Value;
                DateTime hire = dateHire.SelectedDate.Value;

                if (employees == null)
                {

                    employees = new Models.Employees()
                    {
                        LastName = lastName,
                        FirstName = firstName,
                        Patronymic = patronymic,
                        BirthDate = birth,
                        PositionId = positionContext.Positions.Where(x => x.Name == position.SelectedItem.ToString()).First().Id,
                        DepartmentId = departmentContext.Departments.Where(x => x.Name == department.SelectedItem.ToString()).First().Id,
                        HireDate = hire,
                        Email = email.Text,
                        PhoneNumber = phoneNumber.Text,
                        StatusId = statusContext.Status.Where(x => x.Name ==  status.SelectedItem.ToString()).First().Id
                    };
                    MainEmployees.EmployeeContext.Employees.Add(employees);
                }
                else
                {
                    employees.LastName = lastName;
                    employees.FirstName = firstName;
                    employees.Patronymic = patronymic;
                    employees.BirthDate = birth;
                    employees.PositionId = positionContext.Positions.Where(x => x.Name == position.SelectedItem.ToString()).First().Id;
                    employees.DepartmentId = departmentContext.Departments.Where(x => x.Name == department.SelectedItem.ToString()).First().Id;
                    employees.HireDate = hire;
                    employees.Email = email.Text;
                    employees.PhoneNumber = phoneNumber.Text;
                    employees.StatusId = statusContext.Status.Where(x => x.Name == status.SelectedItem.ToString()).First().Id;
                }
                MainEmployees.EmployeeContext.SaveChanges();
                MainWindow.mw.OpenPages(new Pages.Employees.Employees());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка.\n" + ex.Message);
            }
        }
    }
}
