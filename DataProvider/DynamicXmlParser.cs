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
            Dictionary<string, Product> Result = new Dictionary<string, Product>(StringComparer.InvariantCultureIgnoreCase);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(DataXML);

            XmlNodeList productNodes = doc.SelectNodes(string.Format("//{0}", XmlMapping.First().Value.ProductNode));

            Dictionary<int, XmlMapItem> ProductTransformations = XmlMapping.Where(x => x.Value.SourceNodeName.Contains("[Product]")).ToDictionary(a => a.Key, b => b.Value);
            Dictionary<int, XmlMapItem> SubProductTransformations = XmlMapping.Where(x => x.Value.SourceNodeName.Contains("[SubProduct]")).ToDictionary(a => a.Key, b => b.Value);

            //Parse concat transformations -- START
            Dictionary<int, XmlMapItem> ConcatTransformationsTemp = XmlMapping.Where(x => x.Value.TransformationType == "Concat").ToDictionary(a => a.Key, b => b.Value);
            Dictionary<int, XmlMapItem> ConcatTransformations = new Dictionary<int, XmlMapItem>();
            foreach (var item in ConcatTransformationsTemp)
            {
                string[] Transformations = item.Value.SourceNodeName.Split(';');
                for (int i = 1; i < Transformations.Length + 1; i++)
                {
                    XmlMapItem x = new XmlMapItem()
                    {
                        XmlMappingConfigurationID = item.Key * -i,
                        XmlMappingID = item.Value.XmlMappingID,
                        DataSourceName = item.Value.DataSourceName,
                        ProductNode = item.Value.ProductNode,
                        SourceNodeIndex = item.Value.SourceNodeIndex,
                        SourceNodeName = Transformations[i - 1],
                        SubProductNode = item.Value.SubProductNode,
                        TargetAttribute = item.Value.TargetAttribute,
                        TransformationType = item.Value.TransformationType
                    };
                    if (x.SourceNodeName.Contains("[Product]"))
                    {
                        if (ProductTransformations.ContainsKey(item.Key))
                            ProductTransformations.Remove(item.Key);
                    }
                    else if (x.SourceNodeName.Contains("[SubProduct]"))
                    {
                        if (SubProductTransformations.ContainsKey(item.Key))
                            SubProductTransformations.Remove(item.Key);
                    }

                    ConcatTransformations.Add(x.XmlMappingConfigurationID, x);
                }
            }
            foreach (var c in ConcatTransformations)
            {
                if (c.Value.SourceNodeName.Contains("[Product]"))
                    ProductTransformations.Add(c.Key, c.Value);
                if (c.Value.SourceNodeName.Contains("[SubProduct]"))
                    SubProductTransformations.Add(c.Key, c.Value);
            }
            //Parse concat transformations -- END

            foreach (XmlNode pNode in productNodes)
            {
                Product p = new Product();
                Dictionary<string, string> ConcatTransformationValues = new Dictionary<string, string>();

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

                    // Do not assign if it is null
                    if (string.IsNullOrWhiteSpace(AttrValue))
                        continue;

                    if (item.Value.TransformationType == "Concat")
                    {
                        if (!ConcatTransformationValues.ContainsKey(item.Value.SourceNodeName))
                            ConcatTransformationValues.Add(item.Value.SourceNodeName, AttrValue);
                    }
                    else if (p.GetType().GetField(item.Value.TargetAttribute).FieldType.Name == "Int32")
                        p.GetType().GetField(item.Value.TargetAttribute).SetValue(p, int.Parse(AttrValue));
                    else if (p.GetType().GetField(item.Value.TargetAttribute).FieldType.Name == "Double")
                        p.GetType().GetField(item.Value.TargetAttribute).SetValue(p, Convert.ToDouble(AttrValue, new CultureInfo("en-US")));
                    else
                        p.GetType().GetField(item.Value.TargetAttribute).SetValue(p, AttrValue);
                }
                /* Assign Product Attributes -- END */

                /* Find SubProduct Nodes -- START */
                bool SubProductNodeExists = (string.IsNullOrWhiteSpace(XmlMapping.First().Value.SubProductNode)) ? false : true;
                string[] VariantHiearchy = XmlMapping.First().Value.SubProductNode.Split('/');
                XmlNodeList VariantNodes = null;
                XmlNode MiddleLevelNode = null;

                if (SubProductNodeExists && VariantHiearchy.Length > 1)
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
                else if (SubProductNodeExists)
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

                            // Do not assign if it is null
                            if (string.IsNullOrWhiteSpace(AttrValue))
                                continue;

                            if (item.Value.TransformationType == "Concat")
                            {
                                if (!ConcatTransformationValues.ContainsKey(item.Value.SourceNodeName))
                                    ConcatTransformationValues.Add(item.Value.SourceNodeName, AttrValue);
                            }
                            else if (subProduct.GetType().GetField(item.Value.TargetAttribute).FieldType.Name == "Int32")
                                subProduct.GetType().GetField(item.Value.TargetAttribute).SetValue(subProduct, int.Parse(AttrValue));
                            else if (subProduct.GetType().GetField(item.Value.TargetAttribute).FieldType.Name == "Double")
                                subProduct.GetType().GetField(item.Value.TargetAttribute).SetValue(subProduct, Convert.ToDouble(AttrValue, new CultureInfo("en-US")));
                            else
                                subProduct.GetType().GetField(item.Value.TargetAttribute).SetValue(subProduct, AttrValue);
                        }

                        if (!Result.ContainsKey(subProduct.SKU))
                            Result.Add(subProduct.SKU, subProduct);
                    }
                }
                /* Assign SubProduct Atributes -- END */

                /* Assign Concat Transformation Values */
                foreach (var item in ConcatTransformations)
                {
                    string AttrVal = "";

                    if (ConcatTransformationValues.ContainsKey(item.Value.SourceNodeName))
                        AttrVal += ConcatTransformationValues[item.Value.SourceNodeName];

                    string currentVal = (string)p.GetType().GetField(item.Value.TargetAttribute).GetValue(p);
                    if (string.IsNullOrEmpty(currentVal))
                        p.GetType().GetField(item.Value.TargetAttribute).SetValue(p, AttrVal);
                    else
                        p.GetType().GetField(item.Value.TargetAttribute).SetValue(p, currentVal + ">" + AttrVal);
                }

                if (!Result.ContainsKey(p.SKU))
                    Result.Add(p.SKU, p);
            }

            return Result;
        }
    }
}
