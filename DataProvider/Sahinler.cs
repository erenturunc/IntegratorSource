using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IntegratorSource.DataProvider
{
    class Sahinler
    {
        public static Dictionary<string, Product> ParseXML2Products(string DataXML)
        {
            Dictionary<string, Product> Result = new Dictionary<string, Product>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DataXML);

            XmlNodeList productNodes = doc.SelectNodes("//product");

            foreach (XmlNode pNode in productNodes)
            {
                Product p = new Product();

                p.ProviderID = Config.ProviderID;
                p.MemberID = Config.MemberID;

                p.SKU = pNode.SelectSingleNode("code").InnerText.Trim();
                p.ProductName = pNode.SelectSingleNode("name").InnerText.Trim();
                p.Category = pNode.SelectSingleNode("category_path").InnerText.Trim();
                p.Quantity = int.Parse(pNode.SelectSingleNode("stock").InnerText.Trim());
                p.Price = Convert.ToDouble(pNode.SelectSingleNode("price_list").InnerText.Trim(), new CultureInfo("en-US")) * (double)((double)108 / (double)100);
                p.SellingPrice = p.Price;
                p.Brand = pNode.SelectSingleNode("brand").InnerText.Trim();
                p.Description = pNode.SelectSingleNode("detail").InnerText.Trim();


                XmlNodeList imgNodes = pNode.SelectSingleNode("images").SelectNodes("img_item");
                int imgIndex = 0;
                foreach (XmlNode imgNode in imgNodes)
                {
                    imgIndex++;

                    switch (imgIndex)
                    {
                        case 1:
                            p.ImageURL01 = imgNode.InnerText.Trim();
                            break;
                        case 2:
                            p.ImageURL02 = imgNode.InnerText.Trim();
                            break;
                        case 3:
                            p.ImageURL03 = imgNode.InnerText.Trim();
                            break;
                        case 4:
                            p.ImageURL04 = imgNode.InnerText.Trim();
                            break;
                        default:
                            break;
                    }
                }

                if (pNode.SelectSingleNode("subproducts") != null)
                {
                    XmlNodeList variantNodes = pNode.SelectSingleNode("subproducts").SelectNodes("subproduct");
                    foreach (XmlNode varNode in variantNodes)
                    {
                        Product varProduct = Util.DeepClone(p);
                        varProduct.ProviderID = Config.ProviderID;
                        varProduct.MemberID = Config.MemberID;
                        varProduct.ProductGroupSKU = p.SKU;
                        varProduct.SKU = varNode.SelectSingleNode("code").InnerText.Trim();
                        varProduct.Attribute04 = varNode.SelectSingleNode("type2").InnerText.Trim();
                        if (string.IsNullOrWhiteSpace(varProduct.Attribute04) || varProduct.Attribute04 == "0")
                            varProduct.Attribute04 = "Gorseldeki Renk";
                        varProduct.Attribute02 = varNode.SelectSingleNode("type1").InnerText.Trim();
                        varProduct.Quantity = int.Parse(varNode.SelectSingleNode("stock").InnerText.Trim());

                        if (!Result.ContainsKey(varProduct.SKU))
                            Result.Add(varProduct.SKU, varProduct);

                    }
                }

                if (!Result.ContainsKey(p.SKU))
                    Result.Add(p.SKU, p);
            }

            return Result;
        }
    }
}
