using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Concentrator.Tasks.Euretco.RSO.Navision.Helpers
{
  public static class ProductExporterHelper
  {
    /// <summary>
    /// Serialize a product output model into a suitable DOM
    /// </summary>
    /// <param name="productOutputModel">product output model</param>
    /// <returns>A DOM representation of the product output model</returns>
    public static XmlDocument SerializeModel<T>(T productOutputModel)
    {
      var namespaces = new XmlSerializerNamespaces();
      namespaces.Add("", "");

      var stringBuilder = new StringBuilder();
      var serializer = new XmlSerializer(typeof(T));

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
    public Utf8StringWriter(StringBuilder stringBuilder)
      : base(stringBuilder)
    {
    }

    public override Encoding Encoding
    {
      get { return Encoding.UTF8; }
    }
  }
}
