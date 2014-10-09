using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;
using System.CodeDom.Compiler;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using Concentrator.Objects;
using System.Text;
using Concentrator.Web.Services.Base;
using Concentrator.Objects.Models.Orders;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for ZipCodes
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  // [System.Web.Script.Services.ScriptService]
  public class ZipCodes : BaseConcentratorService
  {
    [WebMethod(Description = "Get Zipcodes", BufferResponse = false)]
    public byte[] GetZipcodes()
    {
      using (var unit = GetUnitOfWork())
      {
        var zipCodes = unit.Scope.Repository<ZipCode>().GetAllAsQueryable();


        MemoryStream ms = new MemoryStream();

        XmlSerializer x = new XmlSerializer(typeof(ZipCodes));
        x.Serialize(ms, zipCodes);

        byte[] bytes = ms.ToArray();

        return bytes;
      }
    }
  }
}
