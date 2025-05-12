using SYP.Context;
using SYP.Models;
using SYP.Pages.Employees;
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
        DepartmentContext departmentContext = new DepartmentContext();
        PositionContext positionContext = new PositionContext();

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
                    lbFIO.Content = employee.LastName + " " + employee.FirstName + " " + employee.Patronymic;
                    lbDateBirth.Content = "Дата рождения: " + employee.BirthDate.ToString("d MMMM yyyy");
                    lbDepartment.Content = "Отдел: " + departmentContext.Departments.Where(x => x.Id == employee.DepartmentId).FirstOrDefault().Name;
                    lbPosition.Content = "Должность: " + positionContext.Positions.Where(x => x.Id == employee.PositionId).FirstOrDefault().Name;
                    lbDateHire.Content = "Дата поступления: " + employee.HireDate.ToString("d MMMM yyyy");
                    lbPhone.Content = "Номер телефона: " + employee.PhoneNumber;
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
                bool isOldPasswordValid = !string.IsNullOrWhiteSpace(oldPassword);
                bool isNewPasswordValid = !string.IsNullOrWhiteSpace(newPassword);
                bool isConfirmPasswordValid = !string.IsNullOrWhiteSpace(confirmPassword);

                RegexValidator.ValidateControl(OldPasswordBox, isOldPasswordValid);
                RegexValidator.ValidateControl(NewPasswordBox, isNewPasswordValid);
                RegexValidator.ValidateControl(ConfirmPasswordBox, isConfirmPasswordValid);

                if (!isOldPasswordValid || !isNewPasswordValid || !isConfirmPasswordValid)
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

            bool isUsernameChanged = !string.IsNullOrWhiteSpace(newUsername) && newUsername != currentUser.Username;

            bool isEmailChanged = !string.IsNullOrWhiteSpace(newEmail);
            using (var context = new EmployeeContext())
            {
                var employee = context.Employees.FirstOrDefault(e => e.Id == currentUser.EmployeeId);
                if (employee != null)
                {
                    isEmailChanged = isEmailChanged && newEmail != employee.Email;
                }
            }

            if (!isUsernameChanged && !isEmailChanged)
            {
                MessageBox.Show("Нет изменений для сохранения.");
                return;
            }

            bool isUsernameValid = true;
            bool isEmailValid = true;

            if (isUsernameChanged)
            {
                isUsernameValid = RegexValidator.IsUsernameValid(newUsername);
                RegexValidator.ValidateControl(UserNameTextBox, isUsernameValid);

                if (!isUsernameValid)
                {
                    MessageBox.Show("Имя пользователя должно содержать минимум 4 символа и может включать только латинские буквы, цифры, подчёркивания и точки.");
                    return;
                }
            }

            if (isEmailChanged)
            {
                isEmailValid = RegexValidator.IsEmailValid(newEmail);
                RegexValidator.ValidateControl(EmailTextBox, isEmailValid);

                if (!isEmailValid)
                {
                    MessageBox.Show("Некорректный формат email.");
                    return;
                }
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

                    if (isUsernameChanged)
                    {
                        bool usernameExists = userContext.Users
                            .Any(u => u.Username == newUsername && u.Id != currentUser.Id);

                        if (usernameExists)
                        {
                            MessageBox.Show("Пользователь с таким именем уже существует.");
                            return;
                        }

                        user.Username = newUsername;
                        currentUser.Username = newUsername;
                    }

                    if (isEmailChanged)
                    {
                        var employee = employeeContext.Employees.FirstOrDefault(e => e.Id == user.EmployeeId);
                        if (employee == null)
                        {
                            MessageBox.Show("Связанный сотрудник не найден.");
                            return;
                        }

                        employee.Email = newEmail;
                    }

                    userContext.SaveChanges();
                    employeeContext.SaveChanges();

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
