
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SYP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mw;

        public MainWindow()
        {
            InitializeComponent();
            mw = this;
            OpenPages(new Pages.Main());
        }

        public void OpenPages(Page page)
        {
            frame.Navigate(page);
        }
    }
}