using IntegratorSource.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegratorSource
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                LogHelper.LogWriter.Error("Invalid call. Usage: Engine.exe [Member] [Provider]. Params={0}", string.Join(",", args));
                return;
            }

            string Member = args[0].ToLowerInvariant();
            string Provider = args[1].ToLowerInvariant();
            
            LogHelper.LogWriter.Info("Sourcing has been started : {0} {1}", Member, Provider);
            Config.ReadConfig(Member, Provider);

            Dictionary<string, Product> SourceProductList = new Dictionary<string, Product>();

            StreamReader reader = new StreamReader(@"c:\users\eren.turunc\desktop\brangoxml.xml", Encoding.GetEncoding("iso-8859-9"));
            string DataXML = reader.ReadToEnd();
            reader.Close();

            //string DataXML = Util.ReadFromUri(Config.ProviderProductsXmlUri, Provider);
            Dictionary<int, XmlMapItem> XmlMapping = Sql.AppDataProvider.Get_XmlMapping(Config.MemberID, Config.ProviderID);
            SourceProductList = DataProvider.DynamicXmlParser.ParseXML2Products(DataXML, XmlMapping);

            ProductDataProvider.UpsertProducts(SourceProductList);

        }
    }
}
