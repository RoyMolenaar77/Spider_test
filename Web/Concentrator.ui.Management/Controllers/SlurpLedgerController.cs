using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class SlurpLedgerController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetSlurpLedger)]
    public ActionResult GetList()
    {
      return List(unit => from i in unit.Service<ProductCompetitorLedger>().GetAll()
                             select new
                             {
                               i.ProductCompetitorLedgerID,
                               i.ProductCompetitorPriceID,
                               i.Stock,
                               i.Price,
                               i.CreatedBy,
                               i.CreationTime,
                               i.LastModificationTime,
                               i.LastModifiedBy
                             });
    }

    [RequiresAuthentication(Functionalities.UpdateSlurpLedger)]
    public ActionResult Update(int id)
    {
      return Update<ProductCompetitorLedger>(x => x.ProductCompetitorLedgerID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteSlurpLedger)]
    public ActionResult Delete(int id)
    {
      return Delete<ProductCompetitorLedger>(x => x.ProductCompetitorLedgerID == id);
    }
  }
}
