using System.Windows;
using Atualizador.Views;

namespace Atualizador
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ConfiguracaoView();
        }

        private void BtnEmpresas_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new EmpresasView();
        }
    }
}