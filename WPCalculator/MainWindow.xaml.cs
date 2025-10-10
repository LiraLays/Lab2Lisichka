using System.Windows;
using WPCalculator.ViewModels;

namespace WPCalculator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new WPCalculatorViewModel();
        }
    }
}