using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Enumerations;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductBarcodeController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductBarcode)]
    public ActionResult GetList(int productID)
    {
      return List(unit => from pb in unit.Service<ProductBarcode>().GetAll(p => p.ProductID == productID)
                          select new
                          {
                            pb.ProductID,
                            pb.Barcode,
                            BarcodeType = pb.BarcodeType.HasValue ? pb.BarcodeType.Value : 0
                          });
    }

    [RequiresAuthentication(Functionalities.GetProductBarcode)]
    public ActionResult GetBarcodeTypes()
    {
      List<BarcodeTypes> enums = EnumHelper.EnumToList<BarcodeTypes>();

      return Json(new
      {
        results = (from e in enums
                   select new
                   {
                     ID = (int)e,
                     Name = Enum.GetName(typeof(BarcodeTypes), e)
                   }).ToArray()
      });
    }

    [RequiresAuthentication(Functionalities.CreateProductBarcode)]
    public ActionResult Create()
    {
      return Create<ProductBarcode>();
    }

    [RequiresAuthentication(Functionalities.UpdateProductBarcode)]
    public ActionResult Update(int _ProductID, string _Barcode)
    {
      return Update<ProductBarcode>(c => c.ProductID == _ProductID && c.Barcode == _Barcode);
    }

    [RequiresAuthentication(Functionalities.DeleteProductBarcode)]
    public ActionResult Delete(int _productID, string _barcode)
    {
      return Delete<ProductBarcode>(c => c.ProductID == _productID && c.Barcode == _barcode);
    }
  }
}
