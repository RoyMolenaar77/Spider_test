using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductAttributeGroupController : BaseController
  {
    [RequiresAuthentication(Functionalities.CreateProductAttributeGroup)]
    public ActionResult Create()
    {
      return Create<ProductAttributeGroupMetaData>((unit, metadata) =>
      {
        ((IProductService)unit.Service<Product>()).CreateProductAttributeGroup(metadata, GetPostedLanguages());
      });
    }

    [RequiresAuthentication(Functionalities.GetProductAttributeGroup)]
    public ActionResult Search(string query)
    {
      return Search(unit => from o in unit.Service<ProductAttributeGroupName>().Search(query)
                            .Where(x => x.LanguageID == Client.User.LanguageID)
                            select new
                            {
                              AttributeGroupName = o.Name,
                              o.ProductAttributeGroupID,
                            });
    }

    [RequiresAuthentication(Functionalities.UpdateProductAttributeGroup)]
    public ActionResult Update(int id)
    {
      return Update<ProductAttributeMetaData>(c => c.AttributeID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteProductAttributeGroup)]
    public ActionResult Delete(int id)
    {
      return Delete<ProductAttributeMetaData>(c => c.AttributeID == id);
    }
  }
}
