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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SYP.Pages
{
    /// <summary>
    /// Логика взаимодействия для NotificationItem.xaml
    /// </summary>
    public partial class NotificationItem : UserControl
    {

        public NotificationItem()
        {
            InitializeComponent();
        }

        public void ShowVacationRequestsNotification(int newRequestsCount, bool isAdmin)
        {
            if (isAdmin && newRequestsCount > 0)
            {
                vacationRequests.Content = $"• У вас {newRequestsCount} новая заявка на отпуск";
                vacationRequests.Visibility = Visibility.Visible;
            }
            else
            {
                vacationRequests.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdateVacationRequestsVisibility(bool isAdmin, int newRequestsCount)
        {
            if (isAdmin && newRequestsCount > 0)
            {
                vacationRequests.Content = $"• У вас {newRequestsCount} новая заявка на отпуск";
                vacationRequests.Visibility = Visibility.Visible;
            }
            else
            {
                vacationRequests.Visibility = Visibility.Collapsed;
            }
        }

        public bool HasNotifications()
        {
            return dayOfVacation.Visibility == Visibility.Visible || happyBirthday.Visibility == Visibility.Visible || vacationRequests.Visibility == Visibility.Visible;
        }

        public void ShowBirthdayNotification(string message)
        {
            happyBirthday.Content = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 280
            };
            happyBirthday.Visibility = Visibility.Visible;
        }
        public void HideBirthdayNotification()
        {
            happyBirthday.Visibility = Visibility.Collapsed;
        }

        public void ShowVacationNotification(string message)
        {
            dayOfVacation.Content = message;
            dayOfVacation.Visibility = Visibility.Visible;
        }
        public void HideVacationNotification()
        {
            dayOfVacation.Visibility = Visibility.Collapsed;
        }

        private void CloseNotification(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.3),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, a) =>
            {
                this.Visibility = Visibility.Collapsed;
                RootBorder.Opacity = 1.0;
            };

            RootBorder.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}
