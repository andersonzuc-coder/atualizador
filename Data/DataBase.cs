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
                            Comunicacao = reader["Comunicacao"] != DBNull.Value ? Convert.ToByte(reader["Comunicacao"]) : (byte)0
                        });
                    }
                }
            }

            return lista;
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

        public void AtualizarCliente(Cliente c)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"UPDATE cliente SET 
                        Admin = @Admin,
                        Caixa = @Caixa,
                        Copy = @Copy,
                        Conect = @Conect,
                        Regime_Tributario = @Regime_tributario,
                        data_atualizacao = @DataAtualizacao
                       WHERE Codigo = @Codigo";

                var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Admin", c.Admin);
                cmd.Parameters.AddWithValue("@Caixa", c.Caixa);
                cmd.Parameters.AddWithValue("@Copy", c.Copy);
                cmd.Parameters.AddWithValue("@Conect", c.Conect);
                cmd.Parameters.AddWithValue("@Codigo", c.Codigo);
                cmd.Parameters.AddWithValue("@Regime_tributario", c.Regime_Tributario);
                cmd.Parameters.AddWithValue("@DataAtualizacao", c.DataAtualizacao == DateTime.MinValue ? (object)DBNull.Value : c.DataAtualizacao);

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
