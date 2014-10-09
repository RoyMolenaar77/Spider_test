using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Net;
using System.IO;
using System.Xml;

namespace Concentrator.Objects.Xml
{
  public static class Utility
  {
    /// <summary>
    /// Validates an xdocument against a xsd
    /// </summary>
    /// <param name="document">Document to validate</param>
    /// <param name="xsdUrl">The url (online) to the xsd</param>
    /// <returns>successful</returns>
    public static bool ValidateXDocument(XDocument document, string xsdUrl)
    {
      bool isValid = true;
      WebRequest request = WebRequest.Create(xsdUrl);
      
      using(Stream s = request.GetResponse().GetResponseStream())
      {
        XmlSchemaSet set = new XmlSchemaSet();

        set.Add("", XmlReader.Create(s));


        document.Validate(set, (o, e) =>
                                 {
                                   isValid = false;
                                 });
      }
      return isValid;
    }
  }
}
