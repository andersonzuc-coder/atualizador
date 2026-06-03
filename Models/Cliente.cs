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

        public DateTime Data { get; set; }

        // novos campos
        public string Cnpj { get; set; }
        public string Uf { get; set; }
        public byte Comunicacao { get; set; }
    }
}
