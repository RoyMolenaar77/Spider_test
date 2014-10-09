using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Dashboards;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IPortalService
  {
    /// <summary>
    /// Saves a portal portlet
    /// </summary>
    /// <param name="layoutData"></param>
    void SaveLayout(List<UserPortalPortlet> layoutData);
  }
}
