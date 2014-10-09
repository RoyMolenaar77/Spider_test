using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Templates;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Management;

namespace Concentrator.ui.Management.Controllers
{
    public class TemplateController : BaseController
    {
        //
        // GET: /Template/

      public void CreateTemplate(string url,string templateName, string managePageName,string name, string stateName, string columnsOverride, string filters,string xlsName = "",string controllerAssembly = "Concentrator.ui.Management.Controllers")
      {
        using (var work = GetUnitOfWork())
        {
          var user = Client.User;
          Dictionary<string, GridFilter> filterDictionary = buildFilterDictionary(filters);
          int templateID = createExportTemplate(templateName, stateName, managePageName,user);
          createColumns(templateID, columnsOverride,filterDictionary);
        }
      }

      private Dictionary<string, GridFilter> buildFilterDictionary(string filters)
      {
        Dictionary<string, GridFilter> result = new Dictionary<string, GridFilter>();
        if (!String.IsNullOrEmpty(filters))
        {
          var filterElements = filters.Split(new char[] { ',' });
          foreach (var e in filterElements)
          {
            var fe = e.Split(new char[] { '|' });
            if (fe.Length == 4)
            {
              // we have numeric filter
              GridFilter temp = new GridFilter() { Field = fe[0], Comparison = fe[1], Value = fe[2],FilterType=fe[3] };
              result[temp.Field] = temp;
            }
            else
            {
              if (fe.Length == 3)
              {
                GridFilter temp = new GridFilter() { Field = fe[0], Comparison = "eq", Value = fe[1],FilterType=fe[2] };
                result[temp.Field] = temp;
              }
            }
          }
        }
        return result;
      }

      

      private void createColumns(int templateID, string columnsOverride,Dictionary<string,GridFilter> filters)
      {
        var titles = columnsOverride.TrimEnd(new char[] { ']' }).TrimStart(new char[] { '[' }).Split(new char[] { ',' });
        foreach (var t in titles)
        {
          var columntitle = t.Replace("\\", String.Empty).Replace("\"", String.Empty);
          createTemplateColumn(columntitle, templateID,filters);
        }
      }

      private void createTemplateColumn(string title, int templateID,Dictionary<string,GridFilter> filters)
      {
        using (var work = GetUnitOfWork())
        {
          ExportTemplateColumn t = new ExportTemplateColumn();
          t.ExportTemplate = getExportTemplate(templateID);
          t.Name = title;
          t.SortOrder = "ASC";
          if (filters.ContainsKey(t.Name))
          {
            t.Value = filters[t.Name].Value;
            t.FilterOperator = filters[t.Name].Comparison;
            t.FilterType = filters[t.Name].FilterType;
          }
          work.Service<ExportTemplateColumn>().Create(t);
          work.Save();
        }
      }

      private ExportTemplate getExportTemplate(int templateID)
      {
        using (var work = GetUnitOfWork())
        {
          return work.Service<ExportTemplate>().GetAll().Where(x => x.ExportTemplateID == templateID).FirstOrDefault();
        }
      }

      private int createExportTemplate(string templateName, string stateName, string managePageName, IConcentratorPrincipal user)
      {
        using (var work = GetUnitOfWork())
        {
          ExportTemplate temp = new ExportTemplate();
          ManagementPage mp = getManagementPage(managePageName);
          temp.ManagementPage = mp;
          temp.ManagementPageID = mp.PageID;
          temp.TemplateName = templateName;
          temp.UserID = user.UserID;
          work.Service<ExportTemplate>().Create(temp);
          work.Save();
          return temp.ExportTemplateID;
        }
      }

      private ManagementPage getManagementPage(string managePageName)
      {
        using (var work = GetUnitOfWork())
        {
          return work.Service<ManagementPage>().GetAll().Where(x => x.ID == managePageName).FirstOrDefault();
        }
      }


      public JsonResult GetTemplates(string id)
      {
        using (var work = GetUnitOfWork())
        {
          var userId = Client.User.UserID;
          var page=getManagementPage(id);
          if (page != null)
          {
            var templates = work.Service<ExportTemplate>().GetAll().Where(x => (x.UserID == userId) && (x.ManagementPageID == page.PageID));
            var JsonData = templates.Select(x => new { header = x.TemplateName,dataIndex=x.ExportTemplateID }).ToList();
            return new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = JsonData };
          }
          else
          {
            return new JsonResult();
          }
        }
      }

      public JsonResult GetColumnsForTemplate(string id)
      {
        var userId = Client.User.UserID;
        using (var work = GetUnitOfWork())
        {
          var template = work.Service<ExportTemplate>().GetAll().Where(x => (x.UserID == userId) && (x.TemplateName == id)).FirstOrDefault();
          if (template != null)
          {
            var data = template.ExportTemplateColumns.Select(x => new { name = x.Name,comparison=x.FilterOperator,value=x.Value,type=x.FilterType }).ToList();
            return new JsonResult() { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
          }
          else
          {
            return new JsonResult();
          }
        }
      }
      



      

      

    }

    internal class GridFilter
    {
      public string Comparison { get; set; }
      public string Field { get; set; }
      public string Value { get; set; }
      public string FilterType { get; set; }
    }
}
