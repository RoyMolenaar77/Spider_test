using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Dashboards;

namespace Concentrator.Objects.Services
{
  public class PortalService : Service<Portal>, IPortalService
  {
    public void SaveLayout(List<UserPortalPortlet> layoutData)
    {
      if (layoutData.Count == 0) return; //shortcircuit, nothing to save

      var userPortalPortletRepo = Repository<UserPortalPortlet>();

      int portalID = (int)layoutData.First().PortalID;
      int userID = (int)layoutData.First().UserID;

      userPortalPortletRepo.Delete(c => c.PortalID == portalID && c.UserID == userID);

      layoutData.ForEach((portlet) =>
      {
        userPortalPortletRepo.Add(new UserPortalPortlet
        {
          UserID = portlet.UserID,
          Column = portlet.Column,
          Row = portlet.Row,
          PortalID = portlet.PortalID,
          PortletID = portlet.PortletID
        });
      });
    }

  }
}