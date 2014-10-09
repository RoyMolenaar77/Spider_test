using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Concentrator.ui.Management.Models;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System.Data.Objects.SqlClient;
using System.Linq.Expressions;
using Concentrator.Web.Shared.Models;
using System.Globalization;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Web;

namespace Concentrator.ui.Management.Controllers
{
  public class PriceSetController : BaseController
  {
    [RequiresAuthentication(Functionalities.ViewPriceSet)]
    public ActionResult GetList()
    {
      return List(unit => from s in unit.Service<PriceSet>().GetAll()
                          select new
                          {
                            PriceSetID = s.PriceSetID,
                            Name = s.Name,
                            Description = s.Description,
                            Price = s.Price,
                            DiscountPercentage = s.DiscountPercentage,
                            ConnectorID = s.ConnectorID
                          });
    }

    [RequiresAuthentication(Functionalities.UpdatePriceSet)]
    public ActionResult Update(int id)
    {
      return Update<PriceSet>(c => c.PriceSetID == id);
    }

    [RequiresAuthentication(Functionalities.DeletePriceSet)]
    public ActionResult Delete(int id)
    {
      return Delete<PriceSet>(c => c.PriceSetID == id);
    }

    [RequiresAuthentication(Functionalities.CreatePriceSet)]
    public ActionResult Create()
    {
      return Create<PriceSet>();
    }

    [RequiresAuthentication(Functionalities.ViewPriceSet)]
    public ActionResult Search(string query)
    {
      return Search<PriceSet>(unit => from o in unit.Service<PriceSet>().Search(query)
                                      where o.Name.Contains(query)
                                      select o
                            );
    }
  }
}