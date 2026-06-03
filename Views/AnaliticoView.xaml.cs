using Atualizador.Data;
using System.Linq;
using System.Windows.Controls;

namespace Atualizador.Views
{
    public partial class AnaliticoView : UserControl
    {
        private Database db = new Database();

        public AnaliticoView()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            var clientes = db.GetClientes();
            // número simples de atualizadas vs desatualizadas (data_versao não nulo e maior que um threshold)
            var atualizadas = clientes.Count(c => c.DataVersao != System.DateTime.MinValue);
            var desatualizadas = clientes.Count - atualizadas;

            TbTotal.Text = clientes.Count.ToString();

            TbAtualizadas.Text = atualizadas.ToString();
            TbDesatualizadas.Text = desatualizadas.ToString();

            // preparar dados para gráficos simples (barras proporcionais)
            var byRegime = clientes.GroupBy(c => string.IsNullOrEmpty(c.Regime_Tributario) ? "(sem)" : c.Regime_Tributario)
                                   .Select(g => new { Label = g.Key, Value = g.Count() })
                                   .OrderByDescending(x => x.Value).ToList();

            var maxReg = byRegime.Any() ? byRegime.Max(x => x.Value) : 1;
            IcRegime.ItemsSource = byRegime.Select(x => new { Label = x.Label, Value = x.Value, Height = 120.0 * x.Value / maxReg });

            var byUf = clientes.GroupBy(c => string.IsNullOrEmpty(c.Uf) ? "(sem)" : c.Uf)
                               .Select(g => new { Label = g.Key, Value = g.Count() })
                               .OrderByDescending(x => x.Value).ToList();
            var maxUf = byUf.Any() ? byUf.Max(x => x.Value) : 1;
            IcUf.ItemsSource = byUf.Select(x => new { Label = x.Label, Value = x.Value, Height = 120.0 * x.Value / maxUf });

            // comunicação
            var byCom = clientes.GroupBy(c => string.IsNullOrEmpty(c.Comunicacao) ? "NAO" : c.Comunicacao)
                                .Select(g => new { Label = g.Key, Value = g.Count() })
                                .OrderByDescending(x => x.Value).ToList();
            var maxCom = byCom.Any() ? byCom.Max(x => x.Value) : 1;
            IcComunicacao.ItemsSource = byCom.Select(x => new { Label = x.Label, Value = x.Value, Height = 120.0 * x.Value / maxCom });

            var byData = clientes.Where(c => c.DataVersao != System.DateTime.MinValue)
                                 .GroupBy(c => new { c.DataVersao.Year, c.DataVersao.Month })
                                 .Select(g => new { Label = $"{g.Key.Month:00}/{g.Key.Year}", Value = g.Count() })
                                 .OrderBy(x => x.Label).ToList();
            var maxData = byData.Any() ? byData.Max(x => x.Value) : 1;
            IcDataVersao.ItemsSource = byData.Select(x => new { Label = x.Label, Value = x.Value, Height = 120.0 * x.Value / maxData });
        }
    }
}
