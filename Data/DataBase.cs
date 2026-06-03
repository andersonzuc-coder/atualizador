using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atualizador.Data
{
    using Atualizador.Models;
    using MySql.Data.MySqlClient;
    using System.Data.SqlClient;
    using System.Xml.Linq;

    public class Database
    {
        private string connectionString = "server=02analise;database=Atualizador;uid=root;pwd=090408;";

        public List<Cliente> GetClientes()
        {
            var lista = new List<Cliente>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                var cmd = new MySqlCommand("SELECT Codigo, Nome, Admin, Caixa, Copy, Conect, Regime_Tributario, Data, Cnpj, Uf, Comunicacao FROM cliente", conn);
                var reader = cmd.ExecuteReader();

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
                        Data = reader["Data"] != DBNull.Value ? Convert.ToDateTime(reader["Data"]) : DateTime.MinValue,
                        Cnpj = reader["Cnpj"] != DBNull.Value ? reader["Cnpj"].ToString() : string.Empty,
                        Uf = reader["Uf"] != DBNull.Value ? reader["Uf"].ToString() : string.Empty,
                        Comunicacao = reader["Comunicacao"] != DBNull.Value ? Convert.ToByte(reader["Comunicacao"]) : (byte)0
                    });
                }
            }

            return lista;
        }
        public void AtualizarCliente(Cliente c)
        {
            using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"UPDATE cliente SET 
                        Admin = @Admin,
                        Caixa = @Caixa,
                        Copy = @Copy,
                        Conect = @Conect,
                        Regime_Tributario = @Regime_tributario
                       WHERE Codigo = @Codigo";

                var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Admin", c.Admin);
                cmd.Parameters.AddWithValue("@Caixa", c.Caixa);
                cmd.Parameters.AddWithValue("@Copy", c.Copy);
                cmd.Parameters.AddWithValue("@Conect", c.Conect);
                cmd.Parameters.AddWithValue("@Codigo", c.Codigo);
                cmd.Parameters.AddWithValue("@Regime_tributario", c.Regime_Tributario);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
