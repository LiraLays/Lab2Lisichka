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

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string helpText = @"СПРАВКА - WP Calculator

Как использовать:
1. Напишите программу в поле 'Программа'
2. Укажите целевое условие в поле 'Цель'
3. Нажмите 'Рассчитать WP'

Поддерживаемые операторы:
- Присваивание: x := 5
- Условный оператор: if (условие)
- Последовательность: операторы разделяются переносом строки

Комментарии:
// Однострочный комментарий
/* Многострочный 
   комментарий */

Примеры:
x := 10;
y := x + 5;  // Это комментарий
if (y > 15)
    max := y;

Программа игнорирует комментарии и пустые строки!";

            MessageBox.Show(helpText, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}