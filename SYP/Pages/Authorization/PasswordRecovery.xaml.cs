using SYP.Context;
using SYP.Models;
using System.Net.Mail;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SYP.Pages.Authorization
{
    /// <summary>
    /// Логика взаимодействия для PasswordRecovery.xaml
    /// </summary>
    public partial class PasswordRecovery : Page
    {
        private string recoveryCode;
        private Users foundUser;

        public PasswordRecovery()
        {
            InitializeComponent();
        }

        private void SendRecovery(object sender, RoutedEventArgs e) 
        {
            string input = UserName.Text.Trim();

            using (var userContext = new UserContext())
            using (var empContext = new EmployeeContext())
            {
                foundUser = userContext.Users.FirstOrDefault(u => u.Username == input);

                if (foundUser == null)
                {
                    var employee = empContext.Employees.FirstOrDefault(e => e.Email == input);
                    if (employee != null)
                    {
                        foundUser = userContext.Users.FirstOrDefault(u => u.EmployeeId == employee.Id);
                    }
                }

                if (foundUser == null)
                {
                    MessageBox.Show("Пользователь с таким именем или email не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var foundEmployee = empContext.Employees.FirstOrDefault(e => e.Id == foundUser.EmployeeId);
                if (foundEmployee == null || string.IsNullOrWhiteSpace(foundEmployee.Email))
                {
                    MessageBox.Show("У пользователя не указан email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                recoveryCode = new Random().Next(100000, 999999).ToString();
                Application.Current.Properties["RecoveryCode"] = recoveryCode;
                Application.Current.Properties["UserIdToReset"] = foundUser.Id;

                try
                {
                    SendEmail(foundEmployee.Email, recoveryCode);
                    MessageBox.Show("Код восстановления отправлен на вашу почту.");
                    NavigationService.Navigate(new ResetPassword());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при отправке email: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SendEmail(string toEmail, string code)
        {
            var fromAddress = new MailAddress("pleshkovaanastas@yandex.ru", "SotrudniK");
            var toAddress = new MailAddress(toEmail);
            const string subject = "Код восстановления пароля";
            string body = $"Здравствуйте!\n\nВаш код для восстановления пароля: {code}\n\nЕсли вы не запрашивали восстановление — просто проигнорируйте это письмо.";

            var smtp = new SmtpClient
            {
                Host = "smtp.yandex.ru",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("pleshkovaanastas@yandex.ru", "enrklmiqltflflhx")
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            })
            {
                smtp.Send(message);
            }
        }

        private void BackAuthorization(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Authorization());
        }
    }
}
