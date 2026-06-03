using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atualizador.Models
{
    public class Cliente
    {
        public int Codigo { get; set; }
        public string Nome { get; set; }

        public bool Admin { get; set; }
        public bool Caixa { get; set; }
        public bool Copy { get; set; }
        public bool Conect { get; set; }

        public string Regime_Tributario { get; set; }
        // data de atualização (campo data_atualizacao no banco)
        public DateTime DataAtualizacao { get; set; }

        // data da versão do exe (campo data_versao no banco)
        public DateTime DataVersao { get; set; }
        public string Cnpj { get; set; }
        public string Uf { get; set; }
        // agora armazena "SIM" / "NAO"
        public string Comunicacao { get; set; }
    }
}
