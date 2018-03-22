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
    class ProductDataProvider
    {
        public static void UpsertProducts(Dictionary<string, Product> Products)
        {
            string connString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string tableName = "#tmp_" + Util.RandomString(10);
            SqlCommand cmd;

            DataTable dataTable = Util.ConvertProductDictionaryToTable(Products);

            // connect to SQL
            using (SqlConnection connection =
                    new SqlConnection(connString))
            {
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                // set the destination table name
                bulkCopy.DestinationTableName = tableName;


                //Column mapping
                foreach (var field in typeof(Product).GetFields())
                    bulkCopy.ColumnMappings.Add(field.Name, field.Name);
                

                connection.Open();

                // Create temp product table
                string createTempTableQuery = "SELECT * INTO "+tableName+" FROM(SELECT * FROM Product WHERE ProductID IS NULL) a";
                cmd = new SqlCommand(createTempTableQuery, connection);
                cmd.ExecuteNonQuery();

                // write the data in the "dataTable"
                bulkCopy.WriteToServer(dataTable);

                // Upsert
                string upsertQuery = GetUpsertCommand(tableName);
                cmd = new SqlCommand(upsertQuery, connection);
                cmd.ExecuteNonQuery();

                // Delete rest of the products
                string deleteQuery = "DELETE FROM Product WHERE MemberID = {0} AND ProviderID = {1} AND SKU NOT IN (SELECT SKU FROM {2})";
                deleteQuery = string.Format(deleteQuery, Config.MemberID, Config.ProviderID, tableName);
                cmd = new SqlCommand(deleteQuery, connection);
                cmd.ExecuteNonQuery();

                connection.Close();
            }
            // reset
            dataTable.Clear();
        }

        private static string GetUpsertCommand(string TableName)
        {
            string Result = @"MERGE Product AS TARGET
USING {0} AS SOURCE
ON(
    TARGET.[SKU] = SOURCE.SKU

    AND TARGET.ProviderID = SOURCE.ProviderID

    AND TARGET.MemberID = SOURCE.MemberID
    )
--When records are matched, update
--the records if there is any change
WHEN MATCHED AND TARGET.ProductName<> SOURCE.ProductName
OR TARGET.Price<> SOURCE.Price
OR TARGET.SellingPrice<> SOURCE.SellingPrice
OR TARGET.Category<> SOURCE.Category THEN
UPDATE SET TARGET.ProductName = SOURCE.ProductName, 
TARGET.Price = SOURCE.Price,
TARGET.SellingPrice = SOURCE.SellingPrice,
TARGET.Category = SOURCE.Category
--When no records are matched, insert
--the incoming records from source
--table to target table
WHEN NOT MATCHED BY TARGET THEN
INSERT(ProviderID, MemberID, Category, GlobalBarcode, SKU, Brand, ProductName, ProductGroupSKU, Attribute01, Attribute02, Attribute03, Attribute04, Attribute05, Attribute06, Attribute07, Attribute08, Attribute09, Attribute10, Attribute11, Attribute12, Attribute13, Attribute14, Attribute15, Attribute16, Attribute17, Attribute18, Attribute19, Attribute20, Description, OfferNote, Condition, Quantity, Price, SellingPrice, FreeShipping, ShippingProviders, DeliveryTime, ImageURL01, ImageURL02, ImageURL03, ImageURL04, ImageURL05, ImageURL06, ImageURL07, ImageURL08, ImageURL09, ImageURL10)
VALUES(SOURCE.ProviderID, SOURCE.MemberID, SOURCE.Category, SOURCE.GlobalBarcode, SOURCE.SKU, SOURCE.Brand, SOURCE.ProductName, SOURCE.ProductGroupSKU, SOURCE.Attribute01, SOURCE.Attribute02, SOURCE.Attribute03, SOURCE.Attribute04, SOURCE.Attribute05, SOURCE.Attribute06, SOURCE.Attribute07, SOURCE.Attribute08, SOURCE.Attribute09, SOURCE.Attribute10, SOURCE.Attribute11, SOURCE.Attribute12, SOURCE.Attribute13, SOURCE.Attribute14, SOURCE.Attribute15, SOURCE.Attribute16, SOURCE.Attribute17, SOURCE.Attribute18, SOURCE.Attribute19, SOURCE.Attribute20, SOURCE.Description, SOURCE.OfferNote, SOURCE.Condition, SOURCE.Quantity, SOURCE.Price, SOURCE.SellingPrice, SOURCE.FreeShipping, SOURCE.ShippingProviders, SOURCE.DeliveryTime, SOURCE.ImageURL01, SOURCE.ImageURL02, SOURCE.ImageURL03, SOURCE.ImageURL04, SOURCE.ImageURL05, SOURCE.ImageURL06, SOURCE.ImageURL07, SOURCE.ImageURL08, SOURCE.ImageURL09, SOURCE.ImageURL10)
;
--When there is a row that exists in target table and
--same record does not exist in source table
--then delete this record from target table
--WHEN NOT MATCHED BY SOURCE THEN
--DELETE; ";

            return string.Format(Result, TableName);
        }
    }
}
