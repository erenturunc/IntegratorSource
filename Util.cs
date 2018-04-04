using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace IntegratorSource
{
    class Util
    {
        public static string SendHttpGetRequest(string Url)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        public static string SendHttpPostRequest(string Url, string Body)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);

            var data = Encoding.ASCII.GetBytes(Body);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        internal static string ReadFromUri(string providerProductsXmlUri, string member, string provider)
        {
            string folder = "tmp" + "/" + member;
            string filePath = folder + "/" + provider + DateTime.Now.ToString("yyyyMMddHHmmss") + ".dat";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(providerProductsXmlUri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(resStream, Encoding.GetEncoding("iso-8859-9"));
            string DataXML = reader.ReadToEnd();

            StreamWriter writer = new StreamWriter(filePath);
            writer.Write(DataXML);
            writer.Close();

            return DataXML;
        }

        public static CurrencyRates ParseOpenExchangeRateCurrencies(string Path)
        {
            CurrencyRates rates = new CurrencyRates();

            StreamReader reader = new StreamReader(Path);
            string jsonData = reader.ReadToEnd();
            reader.Close();

            dynamic data = JsonConvert.DeserializeObject(jsonData);
            rates.USD = Convert.ToDouble(data.rates.USD);
            rates.TRY = Convert.ToDouble(data.rates.TRY);
            rates.IRR = Convert.ToDouble(data.rates.IRR);
            rates.AED = Convert.ToDouble(data.rates.AED);

            return rates;
        }

        public static Product DeepClone(Product obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (Product)formatter.Deserialize(ms);
            }
        }

        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        internal static DataTable ConvertProductDictionaryToTable(Dictionary<string, Product> Products)
        {
            DataTable dt = new DataTable();

            foreach (var field in typeof(Product).GetFields())
            {
                dt.Columns.Add(field.Name);
            }

            foreach (var item in Products)
            {
                DataRow dr = dt.NewRow();
                foreach (var attr in item.Value.GetType().GetFields())
                {
                    dr[attr.Name] = attr.GetValue(item.Value);
                }

                dt.Rows.Add(dr);
            }

            return dt;
        }
    }
}
