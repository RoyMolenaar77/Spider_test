using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Localization;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class LanguageController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetLanguage)]
    public ActionResult GetLanguages()
    {
      return Json(new
      {
        languages = SimpleList<Language>(l => new
        {
          l.Name,
          ID = l.LanguageID
        })
      });
    }

    [RequiresAuthentication(Functionalities.GetLanguage)]
    public ActionResult GetList()
    {
      return List(unit => from c in unit.Service<Language>().GetAll()
                          select new
                          {
                            ID = c.LanguageID,
                            c.Name,
                            c.DisplayCode
                          });

    }

    [RequiresAuthentication(Functionalities.CreateLanguage)]
    public ActionResult Create()
    {
      return Create<Language>();
    }

    [RequiresAuthentication(Functionalities.UpdateLanguage)]
    public ActionResult Update(int id)
    {
      return Update<Language>(c => c.LanguageID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteLanguage)]
    public ActionResult Delete(int id)
    {
      return Delete<Language>(c => c.LanguageID == id);
    }

    [RequiresAuthentication(Functionalities.GetLanguage)]
    public ActionResult GetLanguagesObject()
    {
      using (var unit = GetUnitOfWork())
      {
        var lang = unit.Service<Language>().GetAll().Select(c => c.Name).ToArray();

        return Json(new
        {
          languages = lang
        });
      }
    }

    [RequiresAuthentication(Functionalities.GetLanguage)]
    public ActionResult Search(string query)
    {
      using (var unit = GetUnitOfWork())
      {
        var languages = (from l in unit.Service<Language>().GetAll(c => c.Name.Contains(query) || c.LanguageID.ToString().Contains(query))
                         select new
                         {
                           l.Name,
                           l.LanguageID
                         }).ToList();

        return Json(new
        {
          results = languages
        });
      }
    }
  }
}
