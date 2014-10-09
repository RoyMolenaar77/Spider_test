using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Media;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class MediaTypeController : BaseController
  {
    [RequiresAuthentication(Functionalities.CreateMediaType)]
    public ActionResult Create(string type)
    {
      using (var unit = GetUnitOfWork())
      {
        var Type = unit.Service<MediaType>().Get(x => x.Type.Trim() == type.Trim());
        if (Type == null)
        {
          return Create<MediaType>();
        }
        else
        {
          return Failure("MediaType with the same name already exists");
        }
      }
    }

    [RequiresAuthentication(Functionalities.GetMediaType)]
    public ActionResult Search(string query)
    {
      if (query == null)
      {
        //return everything
        return Search(unit => from o in unit.Service<MediaType>().GetAll()
                              select new
                              {
                                o.TypeID,
                                o.Type
                              });
      }
      return Search(unit => from o in unit.Service<MediaType>().Search(query)
                            select new
                            {
                              o.TypeID,
                              o.Type
                            });
    }

  }
}
