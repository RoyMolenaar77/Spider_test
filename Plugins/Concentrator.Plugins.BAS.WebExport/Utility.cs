using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using log4net;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  public class Utility
  {
    internal static int GetConcentratorProductID(Connector connector, XElement r, ILog log)
    {
      int concentratorProductID;
      if (connector.UseConcentratorProductID)
        concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
      else
      {
        Int32.TryParse(r.Attribute("CustomProductID").Value, out concentratorProductID);
        if (concentratorProductID == 0)
        {
          log.WarnFormat("Skipping product '{0}', because it has a non-numeric Custom Product ID ",
                         r.Attribute("CustomProductID").Value);
          return concentratorProductID;
        }
      }
      return concentratorProductID;
    }
  }

  public enum ImageType
  {
    ProductImage,
    ProductGroupImage,
    BrandImage
  }
}
