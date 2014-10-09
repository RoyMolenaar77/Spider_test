using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Web.Models;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.ui.Management.Models;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Web.Script.Serialization;


namespace Concentrator.ui.Management.Controllers
{
  public class ContentController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetContent)]
    public ActionResult GetMediaAndSpecifications(ContentPortalFilter filter)
    {
      MergeSession(filter, ContentPortalFilter.SessionKey);
      using (var unit = GetUnitOfWork())
      {
        var serviceResult = ((IContentService)unit.Service<Content>()).GetMissing(filter.Connectors, filter.Vendors, filter.BeforeDate, filter.AfterDate, filter.OnDate, filter.IsActive, filter.ProductGroups, filter.Brands, filter.LowerStockCount, filter.GreaterStockCount, filter.EqualStockCount, filter.Statuses);

        if (serviceResult.Key > 0)
        {
          MediaAndSpecificationsModel model = new MediaAndSpecificationsModel();
          var content = serviceResult.Value;

          model.ProductWithoutMediaUrlAndVideo = content.Where(c => !c.YouTube).Count();
          model.ProductsWithoutImage = content.Where(c => !c.Image).Count();
          model.ProductsWithoutMedia = content.Where(c => !c.Image && !c.YouTube).Count();
          model.ProductsWithoutSpecs = content.Where(c => !c.Specifications).Count();
          model.ProductsWithoutMediaAndSpecs = content.Where(c => !c.Image && !c.YouTube && !c.Specifications).Count();

          model.TotalProducts = serviceResult.Key;

          return View("MediaAndspecifications", model);
        }
        else
        {
          return HtmlError();
        }
      }
    }

    [RequiresAuthentication(Functionalities.UpdateContent)]
    public ActionResult Update(int _ProductID, int _ConnectorID)
    {
      return Update<Content>(c => c.ProductID == _ProductID && c.ConnectorID == _ConnectorID);
    }
  }
}
