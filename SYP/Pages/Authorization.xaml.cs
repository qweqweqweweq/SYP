using Microsoft.EntityFrameworkCore;
using SYP.Context;
using SYP.Models;
using System.Windows;
using System.Windows.Controls;

namespace SYP.Pages
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
                    MessageBox.Show("Введите логин.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль.");
                    return;
                }

                using (var usersContext = new UserContext())
                {
                    var findUser = usersContext.Users.FirstOrDefault(x => x.Username == username && x.Password == password);
                    if (findUser != null)
                    {
                        MainWindow.mw.SetCurrentUser(findUser);
                        MainWindow.mw.OpenPages(new Main());
                    }
                    else
                    {
                        MessageBox.Show("Неверное имя пользователя или пароль.");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка при авторизации.\n" + ex.Message);
            }
        }

        private void ForgotPassword(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
