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

namespace SYP.Pages
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        EmployeeContext employeeContext = new EmployeeContext();
        Models.Users currentUser;

        public Settings()
        {
            InitializeComponent();

            currentUser = MainWindow.mw.CurrentUser;

            UserNameTextBox.Text = currentUser.Username;

            using (var context = new EmployeeContext())
            {
                var employee = context.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);
                if (employee != null)
                {
                    EmailTextBox.Text = employee.Email;
                }
            }
        }

        private void OpenMain(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Main());
        }

        private void OpenEmployees(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Employees.Employees());
        }

        private void OpenDepartments(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Departments.Departments());
        }

        private void OpenPositions(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Positions.Positions());
        }

        private void OpenVacations(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Vacations.Vacations());
        }

        private void OpenReports(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Reports());
        }

        private void Logout(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Pages.Authorization.Authorization());
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            try
            {
                if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.");
                    return;
                }
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Новый пароль и подтверждение не совпадают.");
                    return;
                }
                if (newPassword.Length < 8)
                {
                    MessageBox.Show("Новый пароль должен содержать минимум 8 символов.");
                    return;
                }
                using (var context = new UserContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == currentUser.Id);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден.");
                        return;
                    }

                    if (user.Password != oldPassword)
                    {
                        MessageBox.Show("Неверный старый пароль.");
                        return;
                    }

                    user.Password = newPassword;
                    context.SaveChanges();
                    MessageBox.Show("Пароль успешно изменен.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка.\n" + ex.Message);
            }
        }

        private void SavePersonalInfo_Click(object sender, RoutedEventArgs e)
        {
            string newUsername = UserNameTextBox.Text.Trim();
            string newEmail = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newEmail))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            bool isUsernameValid = RegexValidator.IsUsernameValid(newUsername);
            bool isEmailValid = RegexValidator.IsEmailValid(newEmail);

            RegexValidator.ValidateControl(UserNameTextBox, isUsernameValid);
            RegexValidator.ValidateControl(EmailTextBox, isEmailValid);

            if (!isUsernameValid)
            {
                MessageBox.Show("Имя пользователя должно содержать минимум 4 символа и может включать только латинские буквы, цифры, подчёркивания и точки.");
                return;
            }

            if (!isEmailValid)
            {
                MessageBox.Show("Некорректный формат email.");
                return;
            }

            try
            {
                using (var userContext = new UserContext())
                using (var employeeContext = new EmployeeContext())
                {
                    var user = userContext.Users.FirstOrDefault(u => u.Id == currentUser.Id);
                    if (user == null)
                    {
                        MessageBox.Show("Пользователь не найден.");
                        return;
                    }

                    bool usernameExists = userContext.Users
                        .Any(u => u.Username == newUsername && u.Id != currentUser.Id);

                    if (usernameExists)
                    {
                        MessageBox.Show("Пользователь с таким именем пользователя уже существует.");
                        return;
                    }

                    var employee = employeeContext.Employees.FirstOrDefault(e => e.Id == user.EmployeeId);
                    if (employee == null)
                    {
                        MessageBox.Show("Связанный сотрудник не найден.");
                        return;
                    }

                    user.Username = newUsername;
                    employee.Email = newEmail;

                    userContext.SaveChanges();
                    employeeContext.SaveChanges();

                    currentUser.Username = newUsername;

                    MessageBox.Show("Данные успешно обновлены.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении данных: " + ex.Message);
            }
        }
    }
}
