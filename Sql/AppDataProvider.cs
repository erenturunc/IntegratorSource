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

        public static Dictionary<int, XmlMapItem> Get_XmlMapping(int MemberID, int ProviderID)
        {
            Dictionary<int, XmlMapItem> Result = new Dictionary<int, XmlMapItem>();
            string ConnectionString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            SqlConnection conn;
            SqlCommand cmd;
            SqlDataReader reader;
            string query = "Global_Get_Mapping_XMLSource";

            conn = new SqlConnection(ConnectionString);
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                cmd = new SqlCommand(query, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.Add("MemberID", SqlDbType.VarChar).Value = MemberID;
                cmd.Parameters.Add("ProviderID", SqlDbType.VarChar).Value = ProviderID;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    XmlMapItem item = new XmlMapItem();
                    item.DataSourceName = (reader["DataSourceName"] == DBNull.Value) ? "Unknown" : ((string)reader["DataSourceName"]);
                    item.ProductNode = (reader["ProductNode"] == DBNull.Value) ? "" : ((string)reader["ProductNode"]);
                    item.SourceNodeIndex = (reader["SourceNodeIndex"] == DBNull.Value) ? 1 : ((int)reader["SourceNodeIndex"]);
                    item.SourceNodeName = (reader["SourceNodeName"] == DBNull.Value) ? "Unknown" : ((string)reader["SourceNodeName"]);
                    item.SubProductNode = (reader["SubProductNode"] == DBNull.Value) ? "" : ((string)reader["SubProductNode"]);
                    item.TargetAttribute = (reader["TargetAttribute"] == DBNull.Value) ? "Unknown" : ((string)reader["TargetAttribute"]);
                    item.TransformationType = (reader["TransformationType"] == DBNull.Value) ? "Unknown" : ((string)reader["TransformationType"]);
                    item.XmlMappingConfigurationID = (reader["XmlMappingConfigurationID"] == DBNull.Value) ? -1 : ((int)reader["XmlMappingConfigurationID"]);
                    item.XmlMappingID = (reader["XmlMappingID"] == DBNull.Value) ? -1 : ((int)reader["XmlMappingID"]);

                    Result.Add(item.XmlMappingConfigurationID, item);
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
