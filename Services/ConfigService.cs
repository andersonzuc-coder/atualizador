using Atualizador.Models;
using System;
using System.IO;
using System.Text.Json;

namespace Atualizador.Services
{
    public class ConfigService
    {
        private readonly string pasta;
        private readonly string caminho;

        public ConfigService()
        {
            pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Atualizador"
            );

            caminho = Path.Combine(pasta, "config.json");

            // Garante que a pasta existe
            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);
        }

        public void Salvar(ConfigModel config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(caminho, json);
        }

        public ConfigModel Carregar()
        {
            if (!File.Exists(caminho))
                return new ConfigModel();

            var json = File.ReadAllText(caminho);
            return JsonSerializer.Deserialize<ConfigModel>(json);
        }
    }
}