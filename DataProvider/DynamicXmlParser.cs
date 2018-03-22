using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IntegratorSource.DataProvider
{
    class DynamicXmlParser
    {
        public static Dictionary<string, Product> ParseXML2Products(string DataXML, Dictionary<int, XmlMapItem> XmlMapping)
        {
            Dictionary<string, Product> Result = new Dictionary<string, Product>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DataXML);

            XmlNodeList productNodes = doc.SelectNodes(string.Format("//{0}", XmlMapping.First().Value.ProductNode));

            Dictionary<int, XmlMapItem> ProductTransformations = XmlMapping.Where(x => x.Value.SourceNodeName.Contains("[Product]")).ToDictionary(a => a.Key, b => b.Value);
            Dictionary<int, XmlMapItem> SubProductTransformations = XmlMapping.Where(x => x.Value.SourceNodeName.Contains("[SubProduct]")).ToDictionary(a => a.Key, b => b.Value);

            foreach (XmlNode pNode in productNodes)
            {
                Product p = new Product();

                p.ProviderID = Config.ProviderID;
                p.MemberID = Config.MemberID;

                /* Assign Product Attributes -- START */
                foreach (var item in ProductTransformations)
                {
                    string[] NodeHierarchy = item.Value.SourceNodeName.Replace("[Product]/", "").Split('/');
                    XmlNodeList TargetNodes = null;
                    XmlNode TargetNode = null;
                    string AttrValue = string.Empty;

                    if (NodeHierarchy.Length > 1)
                    {
                        TargetNode = pNode.SelectNodes(NodeHierarchy[0])[0];
                        for (int i = 1; i < NodeHierarchy.Length; i++)
                        {
                            if (TargetNode.SelectNodes(NodeHierarchy[i]) != null)
                            {
                                TargetNodes = TargetNode.SelectNodes(NodeHierarchy[i]);
                                TargetNode = TargetNodes[0];
                            }

                        }
                    }
                    else
                    {
                        TargetNodes = pNode.SelectNodes(NodeHierarchy[0]);
                    }

                    if (TargetNodes != null && TargetNodes.Count >= item.Value.SourceNodeIndex)
                        AttrValue = TargetNodes[item.Value.SourceNodeIndex - 1].InnerText.Trim();

                    p.GetType().GetField(item.Value.TargetAttribute).SetValue(p, AttrValue);
                }
                /* Assign Product Attributes -- END */

                /* Find SubProduct Nodes -- START */
                string[] VariantHiearchy = XmlMapping.First().Value.SubProductNode.Split('/');
                XmlNodeList VariantNodes = null;
                XmlNode MiddleLevelNode = null;

                if (VariantHiearchy.Length > 1)
                {
                    MiddleLevelNode = pNode.SelectNodes(VariantHiearchy[0])[0];
                    for (int i = 1; i < VariantHiearchy.Length; i++)
                    {
                        if (MiddleLevelNode != null && MiddleLevelNode.SelectNodes(VariantHiearchy[i]) != null)
                        {
                            VariantNodes = MiddleLevelNode.SelectNodes(VariantHiearchy[i]);
                            MiddleLevelNode = VariantNodes[0];
                        }

                    }
                }
                else
                {
                    VariantNodes = pNode.SelectNodes(VariantHiearchy[0]);
                }
                /* Find SubProduct Nodes -- END */

                /* Assign SubProduct Atributes -- START */
                if (VariantNodes != null)
                {
                    foreach (XmlNode vNode in VariantNodes)
                    {

                        Product subProduct = Util.DeepClone(p);
                        subProduct.ProductGroupSKU = p.SKU;

                        foreach (var item in SubProductTransformations)
                        {
                            string[] NodeHierarchy = item.Value.SourceNodeName.Replace("[SubProduct]/", "").Split('/');
                            XmlNodeList TargetNodes = null;
                            XmlNode TargetNode = null;
                            string AttrValue = string.Empty;

                            if (NodeHierarchy.Length > 1)
                            {
                                TargetNode = vNode.SelectNodes(NodeHierarchy[0])[0];
                                for (int i = 1; i < NodeHierarchy.Length; i++)
                                {
                                    if (TargetNode.SelectNodes(NodeHierarchy[i]) != null)
                                    {
                                        TargetNodes = TargetNode.SelectNodes(NodeHierarchy[i]);
                                        TargetNode = TargetNodes[0];
                                    }
                                }
                            }
                            else
                            {
                                TargetNodes = vNode.SelectNodes(NodeHierarchy[0]);
                            }

                            if (TargetNodes != null && TargetNodes.Count >= item.Value.SourceNodeIndex)
                                AttrValue = TargetNodes[item.Value.SourceNodeIndex - 1].InnerText.Trim();

                            subProduct.GetType().GetField(item.Value.TargetAttribute).SetValue(subProduct, AttrValue);
                        }

                        if (!Result.ContainsKey(subProduct.SKU))
                            Result.Add(subProduct.SKU, subProduct);
                    }
                }
                /* Assign SubProduct Atributes -- END */

                /*

                if (!string.IsNullOrWhiteSpace(VariantHiearchy[0]) && pNode.SelectSingleNode(VariantHiearchy[0]) != null)
                {

                    XmlNodeList variantNodes = null;
                    string Value = string.Empty;
                    for (int i = 0; i < VariantHiearchy.Length; i++)
                    {
                        if (pNode.SelectNodes(NodeHierarchy[i]) != null)
                            TargetNode = pNode.SelectNodes(NodeHierarchy[i]);
                    }

                    if (TargetNode != null && TargetNode.Count >= item.Value.SourceNodeIndex)
                        Value = TargetNode[item.Value.SourceNodeIndex - 1].InnerText.Trim();

                    p.GetType().GetProperty(item.Value.TargetAttribute).SetValue(p, Value);

                    foreach (XmlNode varNode in variantNodes)
                    {
                        

                        foreach (var item in SubProductTransformations)
                        {
                            string[] NodeHierarchy = item.Value.SourceNodeName.Replace("[SubProduct]/", "").Split('/');
                            XmlNodeList TargetNode = null;
                            string Value = string.Empty;
                            for (int i = 0; i < NodeHierarchy.Length; i++)
                            {
                                if (pNode.SelectNodes(NodeHierarchy[i]) != null)
                                    TargetNode = pNode.SelectNodes(NodeHierarchy[i]);
                            }

                            if (TargetNode != null && TargetNode.Count >= item.Value.SourceNodeIndex)
                                Value = TargetNode[item.Value.SourceNodeIndex - 1].InnerText.Trim();

                            p.GetType().GetProperty(item.Value.TargetAttribute).SetValue(p, Value);
                        }

                        if (!Result.ContainsKey(subProduct.SKU))
                            Result.Add(subProduct.SKU, subProduct);

                    }
                }
                */
                if (!Result.ContainsKey(p.SKU))
                    Result.Add(p.SKU, p);
            }

            return Result;
        }
    }
}
