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
using System.Windows.Shapes;

namespace SYP
{
    /// <summary>
    /// Логика взаимодействия для DateRangeDialog.xaml
    /// </summary>
    public partial class DateRangeDialog : Window
    {
        public DateTime? StartDate => StartDatePicker.SelectedDate;
        public DateTime? EndDate => EndDatePicker.SelectedDate;

        public DateRangeDialog()
        {
            InitializeComponent();
        }

        public string SelectedFormat
        {
            get
            {
                if (savePDF.IsChecked == true)
                    return "PDF";
                else if (saveExcel.IsChecked == true)
                    return "Excel";
                else
                    return null;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (StartDate == null || EndDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите обе даты.");
                return;
            }
            if (StartDate > EndDate)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания.");
                return;
            }
            if (SelectedFormat == null)
            {
                MessageBox.Show("Пожалуйста, выберите формат экспорта.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
