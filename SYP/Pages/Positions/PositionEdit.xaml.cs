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
    /// Логика взаимодействия для PositionEdit.xaml
    /// </summary>
    public partial class PositionEdit : Page
    {
        Positions MainPositions;
        Models.Positions positions;

        public PositionEdit(Positions MainPositions, Models.Positions positions)
        {
            InitializeComponent();

            this.MainPositions = MainPositions;
            this.positions = positions;

            if (positions != null )
            {
                lbTitle.Content = "Изменение существующей должности";
                NamePosition.Text = positions.Name;
                Salary.Text = positions.Salary.ToString();
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            MainWindow.mw.OpenPages(new Positions());
        }

        public void Save(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NamePosition.Text == null)
                {
                    MessageBox.Show("Введите наименование должности.");
                    return;
                }
                if (Salary.Text == null)
                {
                    MessageBox.Show("Введите зарплату.");
                    return;
                }

                if (positions == null)
                {
                    positions = new Models.Positions()
                    {
                        Name = NamePosition.Text,
                        Salary = double.Parse(Salary.Text)
                    };
                    MainPositions.PositionContext.Positions.Add(positions);
                }
                else
                {
                    positions.Name = NamePosition.Text;
                    positions.Salary = double.Parse(Salary.Text);
                }
                MainPositions.PositionContext.SaveChanges();
                MainWindow.mw.OpenPages(new Pages.Positions.Positions());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возникла ошибка.\n" + ex.Message);
            }
        }

        public string PositionNameText
        {
            get => NamePosition.Text;
            set => NamePosition.Text = value;
        }

        public string SalaryText
        {
            get => Salary.Text;
            set => Salary.Text = value;
        }
    }
}
