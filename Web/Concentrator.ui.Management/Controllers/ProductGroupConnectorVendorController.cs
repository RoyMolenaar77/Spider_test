using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
    public class ProductGroupConnectorVendorController : BaseController
    {
      [RequiresAuthentication(Functionalities.GetProductGroupConnectorVendor)]
      public ActionResult GetList()
      {
        return List(unit => from c in unit.Service<ProductGroupConnectorVendor>().GetAll(l => !Client.User.ConnectorID.HasValue || (Client.User.ConnectorID.HasValue && l.ConnectorID == Client.User.ConnectorID))                                 
                                 select new
                                 {
                                   c.VendorID,
                                   c.ConnectorID,
                                   c.ProductGroupID,
                                   ProductGroupName = c.ProductGroup.ProductGroupLanguages.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID).Name,
                                   c.isPreferredAssortmentVendor,
                                   c.isPreferredContentVendor
                                 });
      }

      [RequiresAuthentication(Functionalities.CreateProductGroupConnectorVendor)]
      public ActionResult Create()
      {
        return Create<ProductGroupConnectorVendor>();
      }
      
      [RequiresAuthentication(Functionalities.UpdateProductGroupConnectorVendor)]
      public ActionResult Update(int _ProductGroupID, int _ConnectorID, int _VendorID)
      {
        return Update<ProductGroupConnectorVendor>(c => c.VendorID == _VendorID && c.ConnectorID == _ConnectorID && c.ProductGroupID == _ProductGroupID);
      }
      
      [RequiresAuthentication(Functionalities.DeleteProductGroupConnectorVendor)]
      public ActionResult Delete(int _ProductGroupID, int _ConnectorID, int _VendorID)
      {
        return Delete<ProductGroupConnectorVendor>(c => c.VendorID == _VendorID && c.ConnectorID == _ConnectorID && c.ProductGroupID == _ProductGroupID);
      }
    }
}
