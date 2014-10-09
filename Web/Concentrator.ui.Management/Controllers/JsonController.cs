using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class JsonController : BaseController
  {
    public string Index(int? vendorID = 51)
    {
      if (!vendorID.HasValue) vendorID = 51;

      using (var unit = GetUnitOfWork())
      {
        var result = ((IProductService)unit.Service<Product>()).GetForIpad(vendorID.Value);

        var products = (from c in result
                        select new
                        {
                          c.ProductID,
                          c.Artnr,
                          c.Chapter,
                          c.Category,
                          c.Subcategory,
                          c.PriceGroup,
                          c.Description1,
                          c.Description2,
                          c.PriceNL,
                          c.PriceBE,
                          c.VatExcl,
                          Image = string.Empty
                        });

        JavaScriptSerializer ser = new JavaScriptSerializer();
        ser.MaxJsonLength = int.MaxValue;
        var jsonStringResult = ser.Serialize(products);

        return jsonStringResult;
      }
    }
  }
}
