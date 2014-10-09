using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport
{
  public static class ProductExporterHelper
  {
    /// <summary>
    /// Serialize product ID's from a list of products in a format that can be substituted in a database query
    /// </summary>
    /// <param name="configuredProducts">A list of configured products</param>
    /// <param name="useAllProducts">Specifies whether all products or only configurable products have to be taken into account</param>
    /// <returns>A serialized list of product ID's</returns>
    public static string SerializeProductsToSqlParm(IEnumerable<ConfiguredProductDBModel> configuredProducts, bool useAllProducts)
    {
      var productIDs = (from p in configuredProducts
                        where p.IsConfigurable || useAllProducts
                        select p.ProductID).ToArray();
      var serializedProductIDs = "";
      if (productIDs.Length > 0)
      {
        serializedProductIDs = String.Join(", ", productIDs);
      }
      return serializedProductIDs;
    }

    /// <summary>
    /// Serialize product ID's from a list of products in a format that can be substituted in a database query
    /// </summary>
    /// <param name="productIDs">A list of ProductIDs</param>
    /// <returns>A serialized list of product ID's</returns>
    public static string SerializeProductsToSqlParm(IEnumerable<Int32> productIDs)
    {
      var serializedProductIDs = "";
      var listOfProductIds = productIDs as int[] ?? productIDs.ToArray();

      if (listOfProductIds.Any())
      {
        serializedProductIDs = String.Join(", ", listOfProductIds);
      }
      return serializedProductIDs;
    }

    /// <summary>
    /// Serialize a product output model into a suitable DOM
    /// </summary>
    /// <param name="productOutputModel">product output model</param>
    /// <returns>A DOM representation of the product output model</returns>
    public static XmlDocument SerializeModel(ProductBizTalkModel productOutputModel)
    {
      var namespaces = new XmlSerializerNamespaces();
      namespaces.Add("", "");
      
      var stringBuilder = new StringBuilder();
      var serializer = new XmlSerializer(typeof(ProductBizTalkModel));

      var stringWriter = new Utf8StringWriter(stringBuilder);

      serializer.Serialize(stringWriter, productOutputModel, namespaces);

      stringWriter.Close();
      
      var repositoryDom = new XmlDocument();
      
      repositoryDom.LoadXml(stringBuilder.ToString());
      
      return (repositoryDom);
    }
  }

  public class Utf8StringWriter : StringWriter
  {
    public Utf8StringWriter(StringBuilder stringBuilder) :base (stringBuilder)
    {
    }

    public override Encoding Encoding
    {
      get { return Encoding.UTF8; }
    }
  }
}