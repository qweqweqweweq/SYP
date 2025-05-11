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
    /// Логика взаимодействия для PasswordRecovery.xaml
    /// </summary>
    public partial class PasswordRecovery : Page
    {
        public PasswordRecovery()
        {
            InitializeComponent();
        }

        private void SendRecovery(object sender, RoutedEventArgs e)
        {

        }

        private void BackAuthorization(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new Authorization());
        }
    }
}
