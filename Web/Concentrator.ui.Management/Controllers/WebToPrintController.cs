using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using Concentrator.Objects.Web;
using Concentrator.Objects;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.WebToPrint;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.ui.Management.Controllers
{
  public class WebToPrintController : BaseController
  {
    #region Bindings
    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetBindings()
    {
      return SimpleList((unit) => from b in GetUnitOfWork().Service<WebToPrintBinding>().GetAll()
                                  select new
                                  {
                                    b.BindingID,
                                    b.Name,
                                    b.QueryText
                                  });
    }

    [RequiresAuthentication(Functionalities.CreateWebToPrint)]
    public ActionResult CreateBinding()
    {
      return Create<WebToPrintBinding>(CheckForFields);
    }

    private void CheckForFields(IServiceUnitOfWork unit, WebToPrintBinding wtpb)
    {
      try
      {
        if (wtpb.WebToPrintBindingFields == null)
        {
          wtpb.WebToPrintBindingFields = new List<WebToPrintBindingField>();
        }
        if (wtpb.WebToPrintBindingFields.Count != 0)
        {
          unit.Service<WebToPrintBindingField>().Delete(wtpb.WebToPrintBindingFields);
          wtpb.WebToPrintBindingFields = new List<WebToPrintBindingField>();
          unit.Save();
        }

        string query = wtpb.QueryText;
        wtpb.Query = "";
        if (query.ToLower().Contains("select") && query.ToLower().Contains("from"))
        {
          int fromindex = query.ToLower().LastIndexOf("from");
          int offset = 7;
          if (query.ToLower().Contains("distinct"))
            offset += 9;

          List<string> selects = new List<string>();
          string subString = query.Substring(offset, fromindex - offset);
          int bracketcount = 0;
          int lastIndex = 0;
          for (int i = 0; i < subString.Length; i++)
          {
            if (subString[i] == '(')
              bracketcount++;
            if (subString[i] == ')')
              bracketcount--;

            if (subString[i] == ',' && bracketcount == 0)
            {
              selects.Add(subString.Substring(lastIndex, i - lastIndex).Trim());
              lastIndex = i + 1;
            }


          }
          selects.Add(subString.Substring(lastIndex, subString.Length - lastIndex).Trim());

          string[] selectwords = selects.ToArray();
          string newquery = "select ";
          if (query.ToLower().Contains("distinct"))
            newquery += "distinct ";


          for (int i = 0; i < selectwords.Length; i++)
          {
            string word = selectwords[i].Trim();
            string name = word;
            int asIndex = word.ToLower().LastIndexOf("as");
            if (asIndex >= 0)
            {
              name = word.Substring((asIndex + 3), word.Length - (asIndex + 3));
            }
            WebToPrintBindingField wtpbf = new WebToPrintBindingField()
            {
              Name = name,
              WebToPrintBinding = wtpb,
              Options = 0,
              Type = (byte)BindingFieldType.Unknown + 1
            };
            unit.Service<WebToPrintBindingField>().Create(wtpbf);
            unit.Save();
            if (asIndex >= 0)
            {
              newquery += word.Substring(0, asIndex - 1) + " as '" + wtpbf.FieldID + "'";
            }
            else
            {
              newquery += word + " as '" + wtpbf.FieldID + "'";
            }
            if (i < selectwords.Length - 1)
              newquery += ", ";
          }

          if (query.ToLower().Contains("where"))
          {

            int whereindex = query.ToLower().LastIndexOf("where") + 5;
            newquery += " " + query.Substring(fromindex, whereindex - 5 - (fromindex));

            newquery += "where";
            string[] wherewords = query.Substring(whereindex, query.Length - whereindex).Split('=');
            for (int i = 0; i < wherewords.Length - 1; i++)
            {
              string[] values = wherewords[i].Trim().Split(' ');
              string[] values2 = wherewords[i + 1].Trim().Split(' ');
              WebToPrintBindingField wtpbf = new WebToPrintBindingField() { Name = values2[0], Type = (byte)BindingFieldType.Unknown };
              wtpb.WebToPrintBindingFields.Add(wtpbf);
              unit.Save();
              newquery += " " + values[values.Length - 1] + "=@" + wtpbf.FieldID;
              if (values2.Length > 1)
              {
                newquery += " " + values2[1];
              }
            }
          }
          else
          {
            newquery += " " + query.Substring(fromindex, query.Length - (fromindex));
          }
          wtpb.Query = newquery;
          unit.Save();
        }
        else
        {
          Exception e = new Exception("The query is invalid");
          throw e;
        }

      }
      catch (Exception e)
      {
        throw e;
      }
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult UpdateBinding(int id)
    {
      return Update<WebToPrintBinding>(c => c.BindingID == id, CheckForFields);
    }

    [RequiresAuthentication(Functionalities.DeleteWebToPrint)]
    public ActionResult DeleteBinding(int id)
    {
      return Delete<WebToPrintBinding>(c => c.BindingID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult EditBinding(int id)
    {
      WebToPrintBinding b = this.GetObject<WebToPrintBinding>(c => c.BindingID == id);
      return Json(new
        {
          success = true,
          data = new
          {
            b.BindingID,
            b.Name,
            b.QueryText
          }
        });
    }
    #endregion

    #region Binding Fields
    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetFields(int? bindingid)
    {
      if (bindingid == null)
      {
        bindingid = -1;
      }
      return SimpleList(context => (from b in context.Service<WebToPrintBindingField>().GetAll(c => c.BindingID == bindingid).ToList()
                                   select new
                                   {
                                     b.BindingID,
                                     b.FieldID,
                                     b.Name,
                                     b.Type,
                                     Options = b.GetActiveOptions()
                                   }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.CreateWebToPrint)]
    public ActionResult CreateField(string options)
    {
        return Create<WebToPrintBindingField>(onCreatingAction: (unit, WTPBindingFieldModel) =>
            {
                WTPBindingFieldModel.Options = options.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Sum(x => int.Parse(x));
            });
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult UpdateField(int _FieldID, int _BindingID)
    {
      return Update<WebToPrintBindingField>(c => c.FieldID == _FieldID && c.BindingID == _BindingID);
    }

    [RequiresAuthentication(Functionalities.DeleteWebToPrint)]
    public ActionResult DeleteField(int _FieldID, int _BindingID)
    {
      return Delete<WebToPrintBindingField>(c => c.FieldID == _FieldID && c.BindingID == _BindingID);
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult EditField(int _FieldID, int _BindingID)
    {
      WebToPrintBindingField b = this.GetObject<WebToPrintBindingField>(c => c.FieldID == _FieldID && c.BindingID == _BindingID);
      if (b != null)
      {
        return Json(new
        {
          success = true,
          data = new
          {
            b.FieldID,
            b.BindingID,
            b.Name,
            b.Type,
            Options = b.GetActiveOptions()
          }
        });
      }
      else
      {
        return Json(new
        {
          success = false,
          message = "Failure"
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetFieldTypes()
    {
      Dictionary<int, string> values = new Dictionary<int, string>();

      foreach (int i in Enum.GetValues(typeof(BindingFieldType)))
      {
        string name = Enum.GetName(typeof(BindingFieldType), i);
        values.Add(i, "Input " + name);
        values.Add(i + 1, "Output " + name);
      }

      return Json(new
      {
        success = true,
        results = from a in values
                  select new
                  {
                    a.Key,
                    a.Value
                  }
      });
    }

      [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetFieldOptionsStore()
    {
        return Json(new
        {
            results = (from c in Enums.Get<BindingFieldOptions>().AsQueryable()
                       select new
                       {
                           OptionName = Enum.GetName(typeof(BindingFieldOptions), c),
                           OptionValue = (int)c
                       }
                     )
        });
    }
    #endregion

    #region Projects
    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetProjects()
    {
      return SimpleList((unit) => from p in GetUnitOfWork().Service<WebToPrintProject>().GetAll()
                                  select new
                                  {
                                    p.ProjectID,
                                    p.UserID,
                                    p.Name,
                                    p.Description
                                  });
    }

    [RequiresAuthentication(Functionalities.DeleteWebToPrint)]
    public ActionResult DeleteProject(int id)
    {
      return Delete<WebToPrintProject>(c => c.ProjectID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult EditProject(int id)
    {
      WebToPrintProject p = this.GetObject<WebToPrintProject>(c => c.ProjectID == id);
      return Json(new
      {
        success = true,
        data = new
        {
          p.ProjectID,
          p.UserID,
          p.Name,
          p.Description
        }
      });
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult UpdateProject(int id)
    {
      return Update<WebToPrintProject>(c => c.ProjectID == id);//, delegate(IServiceUnitOfWork unit, WebToPrintProject wtpb) {});
    }
    #endregion

    #region Pages
    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetPages(int? projectID)
    {
      if (projectID == null)
      {
        projectID = -1;
      }
      return SimpleList((unit) => from p in GetUnitOfWork().Service<WebToPrintPage>().GetAll(c => c.ProjectID == projectID)
                                  select new
                                  {
                                    p.PageID,
                                    p.ProjectID,
                                    p.Name,
                                    Data = p.Data
                                  });
    }

    [RequiresAuthentication(Functionalities.DeleteWebToPrint)]
    public ActionResult DeletePage(int id)
    {
      return Delete<WebToPrintPage>(c => c.PageID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult EditPage(int id)
    {
      WebToPrintPage p = this.GetObject<WebToPrintPage>(c => c.PageID == id);
      return Json(new
      {
        success = true,
        data = new
        {
          p.PageID,
          p.ProjectID,
          p.Name,
          Data = p.Data.ToString()
        }
      });
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult UpdatePage(int id)
    {
      return Update<WebToPrintPage>(c => c.PageID == id);//, delegate(IServiceUnitOfWork unit, WebToPrintProject wtpb) {});
    }
    #endregion

    #region Composites
    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetComposites(int projectID)
    {
      return SimpleList((unit) => from c in GetUnitOfWork().Service<WebToPrintComposite>().GetAll(cc => cc.ProjectID == projectID)
                                  select new
                                  {
                                    c.CompositeID,
                                    c.ProjectID,
                                    c.Name,
                                    c.Data
                                  });
    }

    [RequiresAuthentication(Functionalities.DeleteWebToPrint)]
    public ActionResult DeleteComposite(int id)
    {
      return Delete<WebToPrintComposite>(c => c.CompositeID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult EditComposite(int id)
    {
      WebToPrintComposite c = this.GetObject<WebToPrintComposite>(cc => cc.CompositeID == id);
      return Json(new
      {
        success = true,
        data = new
        {
          c.CompositeID,
          c.ProjectID,
          c.Name,
          c.Data
        }
      });
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult UpdateComposite(int id)
    {
      return Update<WebToPrintComposite>(c => c.CompositeID == id);//, delegate(IServiceUnitOfWork unit, WebToPrintProject wtpb) {});
    }
    #endregion

    #region Documents
    [RequiresAuthentication(Functionalities.GetWebToPrint)]
    public ActionResult GetDocuments(int projectID)
    {
      return SimpleList((unit) => from c in GetUnitOfWork().Service<WebToPrintDocument>().GetAll(cc => cc.ProjectID == projectID)
                                  select new
                                  {
                                    c.DocumentID,
                                    c.ProjectID,
                                    c.Name,
                                    c.Data
                                  });
    }

    [RequiresAuthentication(Functionalities.DeleteWebToPrint)]
    public ActionResult DeleteDocument(int id)
    {
      return Delete<WebToPrintDocument>(c => c.DocumentID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult EditDocument(int id)
    {
      WebToPrintDocument c = this.GetObject<WebToPrintDocument>(cc => cc.DocumentID == id);
      return Json(new
      {
        success = true,
        data = new
        {
          c.DocumentID,
          c.ProjectID,
          c.Name,
          c.Data
        }
      });
    }

    [RequiresAuthentication(Functionalities.UpdateWebToPrint)]
    public ActionResult UpdateDocument(int id)
    {
      return Update<WebToPrintDocument>(c => c.DocumentID == id);//, delegate(IServiceUnitOfWork unit, WebToPrintProject wtpb) {});
    }
    #endregion
  }
}
