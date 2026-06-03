using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Atualizador.Models;
using MySql.Data.MySqlClient;

namespace Atualizador.Data
{
    public class Database
    {
        private string connectionString = "server=02analise;database=Atualizador;uid=root;pwd=090408;";

        public List<Cliente> GetClientes()
        {
            var lista = new List<Cliente>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand("SELECT Codigo, Nome, Admin, Caixa, Copy, Conect, Regime_Tributario, data_atualizacao, data_versao, Cnpj, Uf, Comunicacao FROM cliente", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Cliente
                        {
                            Codigo = Convert.ToInt32(reader["Codigo"]),
                            Nome = reader["Nome"].ToString(),
                            Admin = reader["Admin"] != DBNull.Value && Convert.ToBoolean(reader["Admin"]),
                            Caixa = reader["Caixa"] != DBNull.Value && Convert.ToBoolean(reader["Caixa"]),
                            Copy = reader["Copy"] != DBNull.Value && Convert.ToBoolean(reader["Copy"]),
                            Conect = reader["Conect"] != DBNull.Value && Convert.ToBoolean(reader["Conect"]),
                            Regime_Tributario = reader["Regime_Tributario"] != DBNull.Value ? reader["Regime_Tributario"].ToString() : string.Empty,
                            DataAtualizacao = ParseSafeDate(reader, "data_atualizacao"),
                            DataVersao = ParseSafeDate(reader, "data_versao"),
                            Cnpj = reader["Cnpj"] != DBNull.Value ? reader["Cnpj"].ToString() : string.Empty,
                            Uf = reader["Uf"] != DBNull.Value ? reader["Uf"].ToString() : string.Empty,
                            Comunicacao = reader["Comunicacao"] != DBNull.Value ? reader["Comunicacao"].ToString() : "NAO"
                        });
                    }
                }

                return lista;
            }
        }

        private DateTime ParseSafeDate(MySqlDataReader reader, string field)
        {
            try
            {
                if (reader[field] == DBNull.Value)
                    return DateTime.MinValue;

                var val = reader[field];
                if (val is DateTime dt)
                    return dt;

                DateTime parsed;
                if (DateTime.TryParse(val.ToString(), out parsed))
                    return parsed;

                return DateTime.MinValue;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public void InserirCliente(Cliente c)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO cliente (Codigo, Nome, Cnpj, Uf, Regime_Tributario, Comunicacao) 
                               VALUES (@Codigo, @Nome, @Cnpj, @Uf, @Regime, @Comunicacao)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Codigo", c.Codigo);
                cmd.Parameters.AddWithValue("@Nome", c.Nome ?? string.Empty);
                cmd.Parameters.AddWithValue("@Cnpj", c.Cnpj ?? string.Empty);
                cmd.Parameters.AddWithValue("@Uf", c.Uf ?? string.Empty);
                cmd.Parameters.AddWithValue("@Regime", c.Regime_Tributario ?? string.Empty);
                cmd.Parameters.AddWithValue("@Comunicacao", string.IsNullOrEmpty(c.Comunicacao) ? "NAO" : c.Comunicacao);
                cmd.ExecuteNonQuery();
            }
        }

        public void AtualizarCliente(Cliente c)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // montar UPDATE sem atribuir data_atualizacao quando não houver data (evita tentar gravar NULL em coluna NOT NULL)
                bool temData = c.DataAtualizacao != DateTime.MinValue;
                var sb = new System.Text.StringBuilder();
                sb.Append("UPDATE cliente SET ");
                sb.Append("Admin = @Admin, ");
                sb.Append("Caixa = @Caixa, ");
                sb.Append("Copy = @Copy, ");
                sb.Append("Conect = @Conect, ");
                sb.Append("Regime_Tributario = @Regime_tributario, ");
                if (temData)
                    sb.Append("data_atualizacao = @DataAtualizacao, ");
                sb.Append("Comunicacao = @Comunicacao ");
                sb.Append("WHERE Codigo = @Codigo");

                var cmd = new MySqlCommand(sb.ToString(), conn);
                cmd.Parameters.AddWithValue("@Admin", c.Admin);
                cmd.Parameters.AddWithValue("@Caixa", c.Caixa);
                cmd.Parameters.AddWithValue("@Copy", c.Copy);
                cmd.Parameters.AddWithValue("@Conect", c.Conect);
                cmd.Parameters.AddWithValue("@Codigo", c.Codigo);
                cmd.Parameters.AddWithValue("@Regime_tributario", c.Regime_Tributario ?? string.Empty);
                if (temData)
                    cmd.Parameters.AddWithValue("@DataAtualizacao", c.DataAtualizacao);
                cmd.Parameters.AddWithValue("@Comunicacao", string.IsNullOrEmpty(c.Comunicacao) ? "NAO" : c.Comunicacao);

                cmd.ExecuteNonQuery();
            }
        }

        public void AtualizarDataVersao(int codigo, DateTime dataVersao)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"UPDATE cliente SET data_versao = @DataVersao WHERE Codigo = @Codigo";
                var cmd = new MySqlCommand(sql, conn);
                // se não foi possível determinar a data da versão, usar a data atual
                var dataParaGravar = dataVersao == DateTime.MinValue ? DateTime.Now : dataVersao;
                cmd.Parameters.AddWithValue("@DataVersao", dataParaGravar);
                cmd.Parameters.AddWithValue("@Codigo", codigo);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
