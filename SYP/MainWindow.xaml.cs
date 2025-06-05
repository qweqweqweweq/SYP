using SYP.Models.Calendar;
using SYP.Models.Holiday;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SYP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mw;
        public Models.Users CurrentUser { get; private set; }

        public MainWindow() : this("Default") { }

        public MainWindow(string role)
        {
            InitializeComponent();
            mw = this;
            OpenPages(new Pages.Authorization.Authorization());
        }

        public void OpenPages(Page page)
        {
            frame.Navigate(page);
        }

        public void SetCurrentUser(Models.Users user)
        {
            CurrentUser = user;
        }

        private void Minimize_Click(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

    }
}