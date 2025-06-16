using SYP.Context;
using SYP.Models;
using SYP.Pages.Departments;
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

namespace SYP.Pages.Positions
{
    /// <summary>
    /// Логика взаимодействия для PositionItem.xaml
    /// </summary>
    public partial class PositionItem : UserControl
    {
        Positions MainPositions;
        Models.Positions Position;
        private Models.Users currentUser;

        public PositionItem(Positions MainPositions, Models.Positions Position)
        {
            InitializeComponent();

            this.MainPositions = MainPositions;
            this.Position = Position;

            currentUser = MainWindow.mw.CurrentUser;
            if (currentUser != null && currentUser.Role == "Admin")
            {
                Edit.Visibility = Visibility.Visible;
                Delete.Visibility = Visibility.Visible;
                lbSalary.Visibility = Visibility.Visible;
            }

            lbName.Content = Position.Name;
            lbSalary.Content = "Зарплата: " + Position.Salary + "₽";
        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mw.OpenPages(new PositionEdit(MainPositions, Position));
        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить должность?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var context = new PositionContext())
                    {
                        var positionToDelete = context.Positions.FirstOrDefault(x => x.Id == Position.Id);
                        if (positionToDelete != null)
                        {
                            context.Positions.Remove(positionToDelete);
                            context.SaveChanges();
                            MessageBox.Show("Должность удалена");
                            (this.Parent as Panel).Children.Remove(this);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка.\n" + ex.Message);
            }
        }
    }
}
