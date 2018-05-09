using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace IntegratorSource
{
    public static class Config
    {
        public static string CurrencyRatesJsonFilePath;
        public static string ProviderProductsXmlUri;
        public static int MemberID;
        public static int ProviderID;
        public static CurrencyRates Rates;
        public static Encoding DataSourceEncoding = Encoding.UTF8;

        public static void ReadConfig(string Member, string Provider)
        {
            Dictionary<string, string> ConfigKeyValues = Sql.AppDataProvider.Get_Configuration(Member, Provider);

            if (ConfigKeyValues.ContainsKey("currencyratesjsonfilepath"))
                CurrencyRatesJsonFilePath = ConfigKeyValues["currencyratesjsonfilepath"];
            if (ConfigKeyValues.ContainsKey("providerproductsxmluri"))
                ProviderProductsXmlUri = ConfigKeyValues["providerproductsxmluri"];
            if (ConfigKeyValues.ContainsKey("memberid"))
                MemberID = int.Parse(ConfigKeyValues["memberid"]);
            if (ConfigKeyValues.ContainsKey("providerid"))
                ProviderID = int.Parse(ConfigKeyValues["providerid"]);
            if (ConfigKeyValues.ContainsKey("datasourceencoding"))
                DataSourceEncoding = Encoding.GetEncoding(ConfigKeyValues["datasourceencoding"]);

            Config.Rates = Util.ParseOpenExchangeRateCurrencies(CurrencyRatesJsonFilePath);
        }
    }

    [Serializable]
    public class Product
    {
        public long ProductID;
        public int ProviderID;
        public int MemberID;
        public string Category;
        public string GlobalBarcode;
        public string SKU;
        public string Brand;
        public string ProductName;
        public string ProductGroupSKU;
        public string Attribute01;
        public string Attribute02;
        public string Attribute03;
        public string Attribute04;
        public string Attribute05;
        public string Attribute06;
        public string Attribute07;
        public string Attribute08;
        public string Attribute09;
        public string Attribute10;
        public string Attribute11;
        public string Attribute12;
        public string Attribute13;
        public string Attribute14;
        public string Attribute15;
        public string Attribute16;
        public string Attribute17;
        public string Attribute18;
        public string Attribute19;
        public string Attribute20;
        public string Description;
        public string OfferNote;
        public string Condition;
        public int Quantity;
        public double Price;
        public double SellingPrice;
        public string FreeShipping;
        public string ShippingProviders;
        public string DeliveryTime;
        public string ImageURL01;
        public string ImageURL02;
        public string ImageURL03;
        public string ImageURL04;
        public string ImageURL05;
        public string ImageURL06;
        public string ImageURL07;
        public string ImageURL08;
        public string ImageURL09;
        public string ImageURL10;
    }

    public class Category
    {
        public string Name;
        public int CategoryID;
        public int ParentID;
        public List<Category> Children = new List<Category>();
    }

    public class CurrencyRates
    {
        public double USD = 1;
        public double TRY = -1;
        public double IRR = -1;
        public double AED = -1;
    }

    public class XmlMapItem
    {
        public int XmlMappingID;
        public int XmlMappingConfigurationID;
        public string DataSourceName;
        public string ProductNode;
        public string SubProductNode;
        public string SourceNodeName;
        public int SourceNodeIndex;
        public string TargetAttribute;
        public string TransformationType;
    }

    public static class LogHelper
    {
        public static Logger LogWriter = LogManager.GetCurrentClassLogger();
    }
}
