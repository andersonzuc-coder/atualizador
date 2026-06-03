using Atualizador.Models;
using Atualizador.Services;
using System.Windows;
using System.Windows.Controls;

namespace Atualizador.Views
{
    public partial class ConfiguracaoView : UserControl
    {
        private ConfigService service = new ConfigService();

        public ConfiguracaoView()
        {
            InitializeComponent();
            Carregar();
        }

        private void Carregar()
        {
            var config = service.Carregar();

            TxtPastaRaiz.Text = config.PastaRaiz;
            TxtNuvem.Text = config.PastaNuvem;
            TxtSenha.Password = config.Senha;
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            var config = new ConfigModel
            {
                PastaRaiz = TxtPastaRaiz.Text,
                PastaNuvem = TxtNuvem.Text,
                Senha = TxtSenha.Password
            };

            service.Salvar(config);

            MessageBox.Show("Configuração salva!");
        }
    }
}