using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Dashboards;
using Concentrator.Objects.Model.Users;
using System.Data.SqlClient;

namespace Concentrator.ui.Management.Controllers
{
  public class PortletController : BaseController
  {

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      //return a list of all the roles that belong to a widget
      return List(unit => (from p in unit.Service<Portlet>().GetAll()
                           from r in p.Roles
                           select new
                           {
                             PortletID = p.PortletID,
                             RoleID = r.RoleID,
                             PortletName = p.Name,
                             RoleName = r.RoleName
                           }));

      //return List(unit => from b in unit.Service<Portlet>().GetAll()
      //                    select new
      //                    {
      //                      b.PortletID,
      //                      b.Name
      //                    });


    }

    [RequiresAuthentication(Functionalities.CreatePortletRole)]
    public ActionResult Create(int PortletID, int RoleID)
    {

      using (var unit = GetUnitOfWork())
      {
        try
        {
          //check of je de waardes kunt toevoegen in de database
          var check = (from po in unit.Service<Portlet>().GetAll()
                       from ro in po.Roles
                       where ro.RoleID == RoleID && po.PortletID == PortletID
                       select new
                       {
                         po.PortletID
                       }).ToList();

          if (check.Count != 0)//rolePortlet already excists
          {
            Exception e = new Exception();
            return Failure("Failed to update object ", e, isMultipartRequest: false);
          }

          var p = unit.Service<Portlet>().Get(x => x.PortletID == PortletID);

          var r = unit.Service<Role>().Get(x => x.RoleID == RoleID);

          p.Roles.Add(r);


          unit.Save();

          return Success("Successfully created object", isMultipartRequest: false, needsRefresh: true);

        }
        catch (Exception ex)
        {
          return Failure("Failed to create object ", ex, isMultipartRequest: false);
        }

      }

    }

    public ActionResult Update(int _PortletID, int _RoleID, int? PortletID, int? RoleID)
    {

      //check posted values een waarde hebben
      PortletID = PortletID.HasValue ? PortletID : _PortletID;
      RoleID = RoleID.HasValue ? RoleID : _RoleID;
      
      using (var unit = GetUnitOfWork())
      {
        try
        {


          //check of je de waardes kunt toevoegen in de database
          var check = (from po in unit.Service<Portlet>().GetAll()
                   from ro in po.Roles
                   where ro.RoleID == RoleID && po.PortletID == PortletID
                   select new
                   {
                     po.PortletID
                   }).ToList();

          if (check.Count != 0)//rolePortlet already excists
          {
            Exception e = new Exception();
            return Failure("Failed to update object ", e, isMultipartRequest: false);
          }

          var p = unit.Service<Portlet>().Get(x => x.PortletID == PortletID);

          var r = unit.Service<Role>().Get(x => x.RoleID == RoleID);
          
          p.Roles.Add(r);

          //nu verwijderen we de oude waardes
          var pRemove = unit.Service<Portlet>().Get(x => x.PortletID == _PortletID);
          var rRemove = unit.Service<Role>().Get(x => x.RoleID == _RoleID);

          pRemove.Roles.Remove(rRemove);
          
          unit.Save();

          return Success("Successfully created object", isMultipartRequest: false, needsRefresh: true);

        }
        catch (Exception ex)
        {
          return Failure("Failed to create object ", ex, isMultipartRequest: false);
        }

        
      }
    }

    public ActionResult Delete(int _PortletID, int _RoleID)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var p = unit.Service<Portlet>().Get(x => x.PortletID == _PortletID);

          var r = unit.Service<Role>().Get(x => x.RoleID == _RoleID);

          p.Roles.Remove(r);

          unit.Save();

          return Success("Successfully removed object", isMultipartRequest: false, needsRefresh: false);

        }
        catch (Exception ex)
        {
          return Failure("Failed to remove object ", ex, isMultipartRequest: false);
        }

      }

    }



    //[RequiresAuthentication(Functionalities.cr)]
    public ActionResult SearchPortlets()
    {
      return Search(unit => from p in unit.Service<Portlet>().GetAll()
                            select new
                            {
                              PortletName = p.Name,
                              PortletID = p.PortletID
                            });
    }




  }
}
