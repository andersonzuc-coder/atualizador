using Atualizador.Data;
using Atualizador.Models;
using Atualizador.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
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
        // paginação
        private int _pageSize = 15;
        private int _pageIndex = 0; // zero-based
        private int _totalPages = 0;

        public EmpresasView()
        {
            InitializeComponent();
            Carregar();
            InicializarFiltros();
        }

        private void Carregar()
        {
            _clientes = db.GetClientes();
            _pageIndex = 0;
            AtualizarPagina();
        }

        private void InicializarFiltros()
        {
            // popular comboboxes simples com dados existentes
            _clientes = db.GetClientes();
            CbEmpresa.ItemsSource = _clientes;
            CbEmpresa.DisplayMemberPath = "Nome";

            // popular regimes fixos
            var regimesFixos = new List<string> {
                "Simples Nacional",
                "Simples Nacional Excedido",
                "Lucro Presumido",
                "Lucro Real"
            };
            CbRegime.ItemsSource = regimesFixos;
            CbRegime.SelectedIndex = -1;

            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;
            TbSearch.Text = string.Empty;
        }

        private void AplicarFiltros()
        {
            var lista = _clientes.AsEnumerable();

            // busca por nome
            var termo = TbSearch.Text?.Trim();
            if (!string.IsNullOrEmpty(termo))
            {
                lista = lista.Where(c => c.Nome != null && c.Nome.IndexOf(termo, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // empresa selecionada
            if (CbEmpresa.SelectedItem is Cliente selEmpresa)
            {
                lista = lista.Where(c => c.Codigo == selEmpresa.Codigo);
            }

            // regime selecionado
            if (CbRegime.SelectedItem is string regimeSel)
            {
                lista = lista.Where(c => c.Regime_Tributario == regimeSel);
            }

            if (DpFrom.SelectedDate.HasValue)
            {
                lista = lista.Where(c => c.Data >= DpFrom.SelectedDate.Value);
            }

            if (DpTo.SelectedDate.HasValue)
            {
                lista = lista.Where(c => c.Data <= DpTo.SelectedDate.Value);
            }

            // aplicar paginação sobre a lista filtrada
            var filtrado = lista.ToList();
            _totalPages = (int)Math.Ceiling((double)filtrado.Count / _pageSize);
            _pageIndex = 0;
            AtualizarGridComPagina(filtrado);
        }

        private void LimparFiltros()
        {
            CbEmpresa.SelectedIndex = -1;
            // limpar seleção do regime
            CbRegime.SelectedIndex = -1;
            TbSearch.Text = string.Empty;
            DpFrom.SelectedDate = null;
            DpTo.SelectedDate = null;

            _pageIndex = 0;
            AtualizarPagina();
        }

        private void AtualizarPagina()
        {
            _totalPages = (int)Math.Ceiling((double)_clientes.Count / _pageSize);
            var page = _clientes.Skip(_pageIndex * _pageSize).Take(_pageSize).ToList();
            GridClientes.ItemsSource = page;
            AtualizarInfoPagina();
        }

        private void AtualizarGridComPagina(List<Cliente> filtrado)
        {
            var page = filtrado.Skip(_pageIndex * _pageSize).Take(_pageSize).ToList();
            GridClientes.ItemsSource = page;
            AtualizarInfoPagina(filtrado.Count);
        }

        private void AtualizarInfoPagina(int? totalItems = null)
        {
            var total = totalItems ?? _clientes.Count;
            _totalPages = (int)Math.Ceiling((double)total / _pageSize);
            if (_totalPages == 0) _totalPages = 1;
            TbPagina.Text = $"Página {_pageIndex + 1} de {_totalPages}";
            BtnPrev.IsEnabled = _pageIndex > 0;
            BtnNext.IsEnabled = (_pageIndex + 1) < _totalPages;
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_pageIndex > 0) _pageIndex--;
            AplicarOuAtualizarPagina();
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if ((_pageIndex + 1) < _totalPages) _pageIndex++;
            AplicarOuAtualizarPagina();
        }

        private void AplicarOuAtualizarPagina()
        {
            // se existem filtros aplicados, reaplicar sobre a coleção filtrada
            var termo = TbSearch.Text?.Trim();
            var lista = _clientes.AsEnumerable();
            if (!string.IsNullOrEmpty(termo)) lista = lista.Where(c => c.Nome != null && c.Nome.IndexOf(termo, StringComparison.OrdinalIgnoreCase) >= 0);
            if (CbEmpresa.SelectedItem is Cliente selEmpresa) lista = lista.Where(c => c.Codigo == selEmpresa.Codigo);
            // regime selection for pagination reapply
            if (CbRegime.SelectedItem is string regimeSel2) lista = lista.Where(c => c.Regime_Tributario == regimeSel2);
            if (DpFrom.SelectedDate.HasValue) lista = lista.Where(c => c.Data >= DpFrom.SelectedDate.Value);
            if (DpTo.SelectedDate.HasValue) lista = lista.Where(c => c.Data <= DpTo.SelectedDate.Value);

            var filtrado = lista.ToList();
            _totalPages = (int)Math.Ceiling((double)filtrado.Count / _pageSize);
            if (_totalPages == 0) _totalPages = 1;
            if (_pageIndex >= _totalPages) _pageIndex = _totalPages - 1;

            AtualizarGridComPagina(filtrado);
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

        private void GridClientes_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // manter comportamento padrão simplificado: ordenar a coleção em memória
            var column = e.Column;
            var direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
            column.SortDirection = direction;

            Func<Cliente, object> keySelector = c => c.GetType().GetProperty(column.SortMemberPath).GetValue(c, null);

            IEnumerable<Cliente> sorted;
            if (direction == ListSortDirection.Ascending)
                sorted = ((IEnumerable<Cliente>)GridClientes.ItemsSource).OrderBy(keySelector);
            else
                sorted = ((IEnumerable<Cliente>)GridClientes.ItemsSource).OrderByDescending(keySelector);

            GridClientes.ItemsSource = sorted.ToList();
            e.Handled = true;
        }
    }
}
