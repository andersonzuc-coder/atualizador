using Atualizador.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Atualizador.Services
{
    public class AtualizacaoService
    {
        public void AtualizarClientes(List<Cliente> clientes, string pastaRaiz, string pastaNuvem)
        {
            foreach (var cliente in clientes.Where(c => c.Admin))
            {
                if (cliente.Admin)
                    CopiarExe(pastaRaiz, pastaNuvem, cliente.Codigo, "admin");

                if (cliente.Caixa)
                    CopiarExe(pastaRaiz, pastaNuvem, cliente.Codigo, "caixa");

                if (cliente.Copy)
                    CopiarExe(pastaRaiz, pastaNuvem, cliente.Codigo, "copy");

                if (cliente.Conect)
                    CopiarExe(pastaRaiz, pastaNuvem, cliente.Codigo, "conect");
            }
        }

        private void CopiarExe(string origem, string destino, int codigo, string tipo)
        {
            string arquivoOrigem = Path.Combine(origem, $"{tipo}.exe");
            string arquivoDestino = Path.Combine(destino, $"{codigo}_{tipo}.exe");

            // 🔹 valida se o exe existe
            if (!File.Exists(arquivoOrigem))
            {
                System.Windows.MessageBox.Show($"Arquivo não encontrado: {arquivoOrigem}");
                return;
            }

            // 🔹 cria pasta destino se não existir
            if (!Directory.Exists(destino))
            {
                Directory.CreateDirectory(destino);
            }

            // 🔹 copia sobrescrevendo
            File.Copy(arquivoOrigem, arquivoDestino, true);
        }
    }
}
