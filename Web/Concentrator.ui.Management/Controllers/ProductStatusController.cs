using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Statuses;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductStatusController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductStatus)]
    public ActionResult GetList()
    {
      return List(unit =>
                  from s in unit.Service<AssortmentStatus>().GetAll()
                  select new
                  {
                    s.StatusID,
                    s.Status
                  });
    }

    [RequiresAuthentication(Functionalities.GetProductStatus)]
    public ActionResult Search(string query)
    {
      var q = query.IfNullOrEmpty("").ToLower();
      return SimpleList(unit => from p in unit.Service<AssortmentStatus>().GetAll(c => c.Status.ToLower().Contains(q))
                                select new
                                {
                                  p.StatusID,
                                  ConcentratorStatus = p.Status
                                });
    }

    [RequiresAuthentication(Functionalities.GetProductStatus)]
    public ActionResult ConnectorSearch(string query)
    {
      var q = query.IfNullOrEmpty("").ToLower();
      return SimpleList(unit => from p in unit.Service<AssortmentStatus>().GetAll(c => c.Status.ToLower().Contains(q))
                                select new
                                {
                                  ConnectorStatusID = p.StatusID,
                                  ConnectorStatus = p.Status
                                });
    }


    [RequiresAuthentication(Functionalities.CreateProductStatus)]
    public ActionResult Create()
    {
      return Create<AssortmentStatus>();
    }

    [RequiresAuthentication(Functionalities.DeleteProductStatus)]
    public ActionResult Delete(int id)
    {
      return Delete<AssortmentStatus>(c => c.StatusID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateProductStatus)]
    public ActionResult Update(int id)
    {
      return Update<AssortmentStatus>(c => c.StatusID == id);
    }
  }
}
