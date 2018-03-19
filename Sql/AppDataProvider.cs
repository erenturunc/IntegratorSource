using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratorSource.Sql
{
    class AppDataProvider
    {
        public static Dictionary<string, string> Get_Configuration(string MemberCode, string ProviderCode)
        {
            Dictionary<string, string> Result = new Dictionary<string, string>();
            string ConnectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            SqlConnection conn;
            SqlCommand cmd;
            SqlDataReader reader;
            string query = "Source_Get_Configuration";

            conn = new SqlConnection(ConnectionString);
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                cmd = new SqlCommand(query, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add("MemberCode", SqlDbType.VarChar).Value = MemberCode;
                cmd.Parameters.Add("ProviderCode", SqlDbType.VarChar).Value = ProviderCode;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string key = ((string)reader["ConfigurationKey"]).ToLowerInvariant();
                    string value = ((string)reader["ConfigurationValue"]);
                    if (!Result.ContainsKey(key))
                        Result.Add(key, value);
                }
                reader.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }

            return Result;
        }
    }
}
