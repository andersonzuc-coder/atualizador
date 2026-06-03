using Atualizador.Data;
using Atualizador.Models;
using Atualizador.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Atualizador.Views
{
    /// <summary>
    /// Lógica interna para ConfigService.xaml
    /// </summary>
    public partial class EmpresasView : UserControl
    {
        private Database db = new Database();
        private AtualizacaoService service = new AtualizacaoService();

        public EmpresasView()
        {
            InitializeComponent();
            Carregar();
        }

        private void Carregar()
        {
            GridClientes.ItemsSource = db.GetClientes();
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            var clientes = GridClientes.ItemsSource as List<Cliente>;

            if (clientes == null)
                return;


            foreach (var cliente in clientes)
            {
                db.AtualizarCliente(cliente);
            }


            var configService = new ConfigService();
            var config = configService.Carregar();

            if (string.IsNullOrEmpty(config.PastaRaiz) || string.IsNullOrEmpty(config.PastaNuvem))
            {
                MessageBox.Show("Configure os caminhos primeiro!");
                return;
            }

            // 🔹 executa atualização
            service.AtualizarClientes(clientes, config.PastaRaiz, config.PastaNuvem);

            MessageBox.Show("Atualização enviada com sucesso!");
        }
        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            return ;
        }


        private void GridClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
