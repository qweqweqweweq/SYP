using SYP.Models;
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

namespace SYP.Pages.Elements
{
    /// <summary>
    /// Логика взаимодействия для PositionItem.xaml
    /// </summary>
    public partial class PositionItem : UserControl
    {
        Positions MainPositions;
        Models.Positions Position;

        public PositionItem(Positions MainPositions, Models.Positions Position)
        {
            InitializeComponent();

            this.MainPositions = MainPositions;
            this.Position = Position;

            lbName.Content = Position.Name;
            lbSalary.Content = Position.Salary + "₽";
        }

        private void EditClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void DeleteClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
