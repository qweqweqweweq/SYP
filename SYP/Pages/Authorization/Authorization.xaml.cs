using SYP.Context;
using SYP.Models;
using SYP.Models.PasswHelp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYP.Pages.Authorization
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        private readonly UserContext userContext;

        public Authorization()
        {
            InitializeComponent();

            userContext = new UserContext();
        }

        private async void SignIn(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string username = UserName.Text.Trim();
                string password = PasswordText.Password.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Введите логин.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string hashedPassword = PasswordHelper.ComputeSha256Hash(password);

                using (var usersContext = new UserContext())
                {
                    Users findUser = null;
                    try
                    {
                        findUser = usersContext.Users.FirstOrDefault(x => x.Username == username && x.Password == hashedPassword);
                    }
                    catch (Exception dbEx)
                    {
                        MessageBox.Show($"Ошибка доступа к базе данных:\n{dbEx.Message}", "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (findUser != null)
                    {
                        MainWindow.mw.SetCurrentUser(findUser);
                        MainWindow.mw.OpenPages(new Main());
                    }
                    else
                    {
                        MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при авторизации:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ForgotPassword(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new PasswordRecovery());
        }

        private void PasswordText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SignIn(sender, e);
            }
        }
    }
}
