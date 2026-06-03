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
        private List<Cliente> _clientes = new List<Cliente>();

        public EmpresasView()
        {
            InitializeComponent();
            Carregar();
            InicializarFiltros();
        }

        private void Carregar()
        {
            _clientes = db.GetClientes();
            GridClientes.ItemsSource = _clientes;
        }

        private void InicializarFiltros()
        {
            // popular comboboxes simples com dados existentes
            CbEmpresa.ItemsSource = db.GetClientes();
            CbEmpresa.DisplayMemberPath = "Nome";

            CbRegime.ItemsSource = new List<string> { "Todos", "Simples Nacional", "Presumido", "Real" };
            CbRegime.SelectedIndex = 0;

            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
        }

        private void AplicarFiltros()
        {
            var lista = _clientes.AsEnumerable();

            if (CbEmpresa.SelectedItem is Cliente selEmpresa)
            {
                lista = lista.Where(c => c.Codigo == selEmpresa.Codigo);
            }

            if (CbRegime.SelectedItem is string regime && regime != "Todos")
            {
                lista = lista.Where(c => c.Regime_Tributario == regime);
            }

            if (DpFrom.SelectedDate.HasValue)
            {
                lista = lista.Where(c => c.Data >= DpFrom.SelectedDate.Value);
            }

            if (DpTo.SelectedDate.HasValue)
            {
                lista = lista.Where(c => c.Data <= DpTo.SelectedDate.Value);
            }

            GridClientes.ItemsSource = lista.ToList();
        }

        private void LimparFiltros()
        {
            CbEmpresa.SelectedIndex = -1;
            CbRegime.SelectedIndex = 0;
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;

            GridClientes.ItemsSource = _clientes;
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

        private void BtnAplicarFiltros_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnLimparFiltros_Click(object sender, RoutedEventArgs e)
        {
            LimparFiltros();
        }
    }
}
