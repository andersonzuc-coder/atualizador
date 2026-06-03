using Atualizador.Data;
using Atualizador.Models;
using Atualizador.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
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
            InicializarNovoRegistro();
        }

        private void InicializarNovoRegistro()
        {
            var ufs = new List<string> { "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO" };
            CbNovoUf.ItemsSource = ufs;

            CbNovoRegime.ItemsSource = new List<string> { "Simples Nacional", "Simples Nacional Excedido", "Lucro Presumido", "Lucro Real" };
            // proteger caso controle não exista
            if (this.FindName("CbNovoComunicacao") is System.Windows.Controls.ComboBox cbCom)
            {
                cbCom.ItemsSource = new List<string> { "SIM", "NAO" };
                cbCom.SelectedIndex = 1; // NAO por padrão
            }
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

            // popular UFs a partir dos clientes (ou lista padrão)
            var ufs = _clientes.Select(c => c.Uf).Where(u => !string.IsNullOrEmpty(u)).Distinct().OrderBy(u => u).ToList();
            if (!ufs.Any())
            {
                ufs = new List<string> { "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG","PA","PB","PR","PE","PI","RJ","RN","RS","RO","RR","SC","SP","SE","TO" };
            }
            CbUf.ItemsSource = ufs;
            CbUf.SelectedIndex = -1;

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
                lista = lista.Where(c => (c.Nome != null && c.Nome.IndexOf(termo, StringComparison.OrdinalIgnoreCase) >= 0)
                                           || c.Codigo.ToString() == termo
                                           || (c.Cnpj != null && c.Cnpj.IndexOf(termo, StringComparison.OrdinalIgnoreCase) >= 0));
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

            if (CbUf.SelectedItem is string ufSel)
            {
                lista = lista.Where(c => c.Uf == ufSel);
            }

            if (DpFrom.SelectedDate.HasValue)
            {
                lista = lista.Where(c => c.DataAtualizacao >= DpFrom.SelectedDate.Value);
            }

            if (DpTo.SelectedDate.HasValue)
            {
                lista = lista.Where(c => c.DataAtualizacao <= DpTo.SelectedDate.Value);
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
            if (DpFrom.SelectedDate.HasValue) lista = lista.Where(c => c.DataAtualizacao >= DpFrom.SelectedDate.Value);
            if (DpTo.SelectedDate.HasValue) lista = lista.Where(c => c.DataAtualizacao <= DpTo.SelectedDate.Value);

            var filtrado = lista.ToList();
            _totalPages = (int)Math.Ceiling((double)filtrado.Count / _pageSize);
            if (_totalPages == 0) _totalPages = 1;
            if (_pageIndex >= _totalPages) _pageIndex = _totalPages - 1;

            AtualizarGridComPagina(filtrado);
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            // garantir commit das edições na grid (checkboxes) antes de ler os valores
            GridClientes.CommitEdit(DataGridEditingUnit.Cell, true);
            GridClientes.CommitEdit(DataGridEditingUnit.Row, true);

            var clientes = GridClientes.ItemsSource as List<Cliente>;

            if (clientes == null)
                return;


            var configService = new ConfigService();
            var config = configService.Carregar();

            if (string.IsNullOrEmpty(config.PastaRaiz) || string.IsNullOrEmpty(config.PastaNuvem))
            {
                MessageBox.Show("Configure os caminhos primeiro!");
                return;
            }

            // sempre gravar estado dos checkboxes (Admin, Caixa, Copy, Conect) para cada cliente visível
            foreach (var cliente in clientes)
            {
                // se algum checkbox marcado, definir data_atualizacao agora, caso contrário manter nulo
                if (cliente.Admin || cliente.Caixa || cliente.Copy || cliente.Conect)
                    cliente.DataAtualizacao = DateTime.Now;
                else
                    cliente.DataAtualizacao = DateTime.MinValue; // será gravado como NULL

                db.AtualizarCliente(cliente);
            }

            // gravar data_versao apenas para os que têm algum checkbox marcado
            foreach (var cliente in clientes.Where(c => c.Admin || c.Caixa || c.Copy || c.Conect))
            {
                var fileDate = GetFileModificationDateForClient(config.PastaRaiz, cliente);
                cliente.DataVersao = fileDate == DateTime.MinValue ? DateTime.Now : fileDate;
                db.AtualizarDataVersao(cliente.Codigo, cliente.DataVersao);
            }

            // 🔹 executa atualização
            service.AtualizarClientes(clientes, config.PastaRaiz, config.PastaNuvem);
            MessageBox.Show("Atualização enviada com sucesso!");

            // recarregar informações novas após a atualização
            Carregar();
        }
        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            return;
        }


        private void GridClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void BtnAplicarFiltros_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnAnalitico_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // abrir view analítico em nova janela simples
            var wnd = new System.Windows.Window()
            {
                Title = "Analítico",
                Width = 800,
                Height = 900,
                Content = new AnaliticoView()
            };
            wnd.Show();
        }

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            // habilitar campos para novo registro
            // permitir preenchimento manual do código
            TxtNovoCodigo.IsEnabled = true;
            TxtNovoRazao.IsEnabled = true;
            TxtNovoCnpj.IsEnabled = true;
            CbNovoUf.IsEnabled = true;
            CbNovoRegime.IsEnabled = true;
            if (this.FindName("CbNovoComunicacao") is System.Windows.Controls.ComboBox cbCom2)
                cbCom2.IsEnabled = true;
            BtnSalvarNovo.IsEnabled = true;
        }

        private void BtnSalvarNovo_Click(object sender, RoutedEventArgs e)
        {
            // validar e inserir no banco
            int codigo;
            if (!int.TryParse(TxtNovoCodigo.Text, out codigo))
            {
                MessageBox.Show("Código inválido");
                return;
            }

            var novo = new Cliente
            {
                Codigo = codigo,
                Nome = TxtNovoRazao.Text,
                Cnpj = TxtNovoCnpj.Text,
                Uf = CbNovoUf.SelectedItem as string,
                Regime_Tributario = CbNovoRegime.SelectedItem as string,
                Comunicacao = (this.FindName("CbNovoComunicacao") as System.Windows.Controls.ComboBox)?.SelectedItem as string
            };

            // inserir diretamente via Database
            try
            {
                db.InserirCliente(novo);
                MessageBox.Show("Loja inserida com sucesso");
                // resetar campos e recarregar
                TxtNovoCodigo.Text = string.Empty;
                TxtNovoRazao.Text = string.Empty;
                TxtNovoCnpj.Text = string.Empty;
                CbNovoUf.SelectedIndex = -1;
                CbNovoRegime.SelectedIndex = -1;
                if (this.FindName("CbNovoComunicacao") is System.Windows.Controls.ComboBox cbComReset)
                    cbComReset.SelectedIndex = 1; // NAO
                TxtNovoCodigo.IsEnabled = false;
                TxtNovoRazao.IsEnabled = false;
                TxtNovoCnpj.IsEnabled = false;
                CbNovoUf.IsEnabled = false;
                CbNovoRegime.IsEnabled = false;
                if (this.FindName("CbNovoComunicacao") is System.Windows.Controls.ComboBox cbComReset3)
                    cbComReset3.IsEnabled = false;
                BtnSalvarNovo.IsEnabled = false;
                Carregar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao inserir: " + ex.Message);
            }
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

        private DateTime GetFileModificationDateForClient(string pastaRaiz, Cliente cliente)
        {
            try
            {
                Debug.WriteLine($"GetFileModificationDateForClient: pastaRaiz='{pastaRaiz}', cliente={cliente?.Codigo}");
                if (string.IsNullOrEmpty(pastaRaiz)) return DateTime.MinValue;

                // nomes correspondentes aos checkboxes esperados
                var mapping = new List<(bool Flag, string Name)>
                {
                    (cliente.Admin, "admin"),
                    (cliente.Caixa, "caixa"),
                    (cliente.Copy, "copy"),
                    (cliente.Conect, "conect")
                };

                var candidates = new List<string>();
                foreach (var item in mapping.Where(m => m.Flag))
                {
                    // procurar arquivos no diretório raiz que contenham o sufixo _{name} ou terminem com {name}
                    var files = System.IO.Directory.GetFiles(pastaRaiz);
                    Debug.WriteLine($"Procurando arquivos para token '{item.Name}' - total encontrados na pasta: {files.Length}");
                    foreach (var f in files)
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(f);
                        // aceitar qualquer ocorrência do sufixo (ex: 080420261618_caixa ou caixa) ou nome contido
                        if (name.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                            candidates.Add(f);
                    }
                }

                Debug.WriteLine($"Candidates encontrados: {candidates.Count}");

                if (!candidates.Any())
                {
                    // se não encontrou nenhum arquivo correspondente, tentar pegar qualquer exe na pasta raiz
                    var exes = System.IO.Directory.GetFiles(pastaRaiz, "*.exe");
                    Debug.WriteLine($"Nenhum candidate; exes na pasta: {exes.Length}");
                    if (!exes.Any()) return DateTime.MinValue;
                    var latestExe = exes.OrderByDescending(f => System.IO.File.GetLastWriteTime(f)).First();
                    Debug.WriteLine($"Usando exe mais recente: {latestExe} -> {System.IO.File.GetLastWriteTime(latestExe)}");
                    return System.IO.File.GetLastWriteTime(latestExe);
                }

                // entre os candidatos, tentar extrair a data do nome do arquivo no formato ddMMyyyyHHmm
                foreach (var f in candidates.OrderByDescending(f => System.IO.File.GetLastWriteTime(f)))
                {
                    var name = System.IO.Path.GetFileNameWithoutExtension(f);
                    // procurar prefixo numérico de 12 dígitos (ddMMyyyyHHmm)
                    if (name.Length >= 12)
                    {
                        var prefix = name.Substring(0, 12);
                        if (System.Text.RegularExpressions.Regex.IsMatch(prefix, "^\\d{12}$"))
                        {
                            if (DateTime.TryParseExact(prefix, "ddMMyyyyHHmm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var dt))
                                return dt;
                        }
                    }
                    // se não conseguiu extrair, usar last write time como fallback
                    return System.IO.File.GetLastWriteTime(f);
                }
                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
