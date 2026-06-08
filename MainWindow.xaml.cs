using System.Windows;
using Atualizador.Views;

namespace Atualizador
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // abrir automaticamente a tela de Empresas ao iniciar o sistema
            ContentArea.Content = new EmpresasView();
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new ConfiguracaoView();
        }

        private void BtnEmpresas_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new EmpresasView();
        }

        private void BtnAnaliticoMain_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new AnaliticoView();
        }
    }
}