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

namespace SYP.Pages.Authorization
{
    /// <summary>
    /// Логика взаимодействия для ResetPassword.xaml
    /// </summary>
    public partial class ResetPassword : Page
    {
        public ResetPassword()
        {
            InitializeComponent();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            string enteredCode = RecoveryCode.Text.Trim();
            string newPassword = NewPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(enteredCode) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string storedCode = Application.Current.Properties["RecoveryCode"] as string;
            int? userId = Application.Current.Properties["UserIdToReset"] as int?;

            if (storedCode == null || userId == null)
            {
                MessageBox.Show("Сессия восстановления истекла. Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                MainWindow.mw.OpenPages(new PasswordRecovery());
                return;
            }

            if (enteredCode != storedCode)
            {
                MessageBox.Show("Введён неверный код.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (var userContext = new UserContext())
                {
                    var user = userContext.Users.FirstOrDefault(u => u.Id == userId.Value);
                    if (user != null)
                    {
                        user.Password = newPassword;
                        userContext.SaveChanges();
                        MessageBox.Show("Пароль успешно изменён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainWindow.mw.OpenPages(new Authorization());
                    }
                    else
                    {
                        MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackRecovery(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new PasswordRecovery());
        }
    }
}
