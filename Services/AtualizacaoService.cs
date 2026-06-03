using Atualizador.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
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
            // procurar arquivos na pasta origem que contenham o token do tipo (ex: 080420261618_caixa.exe ou caixa.exe)
            if (!Directory.Exists(origem))
            {
                System.Windows.MessageBox.Show($"Pasta de origem não encontrada: {origem}");
                return;
            }

            var files = Directory.GetFiles(origem, "*.exe");
            var candidates = files.Where(f => Path.GetFileNameWithoutExtension(f).IndexOf(tipo, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (!candidates.Any())
            {
                System.Windows.MessageBox.Show($"Arquivo não encontrado para tipo '{tipo}' em: {origem}");
                return;
            }

            // escolher melhor candidato: tentar extrair prefixo ddMMyyyyHHmm e ordenar por essa data; fallback para LastWriteTime
            string arquivoOrigem = null;
            var best = candidates
                .Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    DateTime dt = File.GetLastWriteTime(f);
                    if (name.Length >= 12)
                    {
                        var prefix = name.Substring(0, 12);
                        if (Regex.IsMatch(prefix, "^\\d{12}$"))
                        {
                            if (DateTime.TryParseExact(prefix, "ddMMyyyyHHmm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsed))
                                dt = parsed;
                        }
                    }
                    return new { File = f, Date = dt };
                })
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();

            arquivoOrigem = best?.File;

            if (string.IsNullOrEmpty(arquivoOrigem) || !File.Exists(arquivoOrigem))
            {
                System.Windows.MessageBox.Show($"Arquivo não encontrado: {Path.Combine(origem, tipo + ".exe")} (candidatos: {candidates.Count})");
                return;
            }

            string arquivoDestino = Path.Combine(destino, $"{codigo}_{tipo}.exe");

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
