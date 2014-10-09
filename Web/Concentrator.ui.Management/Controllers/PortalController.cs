using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Dashboards;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using System.Web.Script.Serialization;
using Concentrator.ui.Management.Models;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class PortalController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetPortal)]
    public ActionResult Get(int id, string name)
    {
      using (var unit = GetUnitOfWork())
      {
        var portal = unit.Service<Portal>().Get(p => p.PortalID == id);

        if (!portal.UserPortals.Any(userPortal => userPortal.UserID == Client.User.UserID))
        {
          portal.UserPortals.Add(new UserPortal
          {
            PortalID = id,
            UserID = Client.User.UserID,
            UserPortalPortlets = new List<UserPortalPortlet>(),
            East = 0,
            West = 0
          });

          unit.Save();
        }

        var widgets = (
          from userPortal in portal.UserPortals.Where(up => up.UserID == Client.User.UserID).ToArray()
          let columns = (
            from userPortalPortlet in userPortal.UserPortalPortlets.Where(c => c.UserID == Client.User.UserID)
            group userPortalPortlet by userPortalPortlet.Column into column
            select new
            {
              column = column.Key,
              portlets = column
                .OrderBy(c => c.Row)
                .Select(a => new
                {
                  a.PortletID,
                  a.Portlet.Name,
                  a.Portlet.Description,
                  a.Portlet.Title,
                  a.Row,
                  a.Column
                })
                .ToArray()
            }).OrderBy(c => c.column).ToArray()
          select new
          {
            portal.Name,
            userPortal.West,
            userPortal.East,
            columns
          }).ToList();

        return Json(new
        {
          portal = widgets,
          id = id,
          JsonRequestBehavior.AllowGet
        });
      }
    }

    [RequiresAuthentication(Functionalities.UpdatePortal)]
    public ActionResult SaveLayout(string portletsSerializedJson, int portalID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var portlets = new JavaScriptSerializer().Deserialize<List<PortletViewModel>>(portletsSerializedJson);

          if (portlets.Count() == 0)
          {
            unit.Service<UserPortalPortlet>().Delete(x => x.UserID == Client.User.UserID && x.PortalID == portalID);
          }

          List<UserPortalPortlet> layoutData = new List<UserPortalPortlet>();
          portlets.ForEach((portlet) =>
          {
            layoutData.Add(new UserPortalPortlet()
            {
              UserID = Client.User.UserID,
              PortalID = portalID,
              PortletID = portlet.PortletID,
              Column = portlet.Column,
              Row = portlet.Row
            });
          });

          ((IPortalService)unit.Service<Portal>()).SaveLayout(layoutData);

          unit.Save();
        }
        return Success("Portal layout saved");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong : ", e);
      }
    }

    [RequiresAuthentication(Functionalities.UpdatePortal)]
    public ActionResult AddPortlet(int portalID, int portletID, int column, int row)
    {
      return Create<UserPortalPortlet>((unit, portlet) =>
      {
        portlet.UserID = Client.User.UserID;
        unit.Service<UserPortalPortlet>().Create(portlet);
      });
    }

    [RequiresAuthentication(Functionalities.GetPortal)]
    public ActionResult GetPortlets(int[] portletIDs)
    {
      using (var unit2 = GetUnitOfWork())
      {

        int roleID = unit2.Service<UserRole>().Get(c => c.UserID == Client.User.UserID).RoleID;

        //get all mapped portlets
        var portletsMapped = (from p in unit2.Service<Portlet>().GetAll()
                              from r in p.Roles
                              orderby (p.PortletID)
                              select new
                              {
                                p.PortletID
                              }).Distinct().ToList();

        //get all portlets mapped to this user
        var allPortletsForRole = (from p in unit2.Service<Portlet>().GetAll()
                                  from r in p.Roles
                                  where r.RoleID == roleID
                                  select new
                                  {
                                    p.PortletID
                                  }).ToList();

        //convert to array
        int[] allPortletsForRoleArray = new int[allPortletsForRole.Count];
        int i = 0;
        foreach (var item in allPortletsForRole)
        {
          allPortletsForRoleArray[i] = item.PortletID;
          i++;
        }


        var portletsNotToShow = (from m in portletsMapped
                                 where (!allPortletsForRoleArray.Contains(m.PortletID))
                                 select new
                                 {
                                   m.PortletID
                                 }).ToList();

        int[] portletsNotToShowArray = new int[portletsNotToShow.Count];
        int j = 0;
        foreach (var item in portletsNotToShow)
        {
          portletsNotToShowArray[j] = item.PortletID;
          j++;
        }

        return List(unit => (from p in unit.Service<Portlet>().GetAll()
                             where (!portletsNotToShowArray.Contains(p.PortletID))
                             select new
                             {
                               p.PortletID,
                               p.Name,
                               p.Title,
                               p.Description
                             }).OrderBy(x => x.Name));
      }

    }

  }
}