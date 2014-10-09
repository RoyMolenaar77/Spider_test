using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.CustomerSpecific.Sapph.Models;
using Concentrator.Web.CustomerSpecific.Sapph.Repositories;
using Concentrator.Web.CustomerSpecific.Sapph.ViewModels;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Concentrator.Web.CustomerSpecific.Sapph.Controllers
{
  public class ProductTypeController : BaseController
  {
    private readonly int[] _vendorIDs = { 50, 51 };
    private const int _defaultVendorID = 50;

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult GetAll()
    {
      using (var unit = GetUnitOfWork())
      {
        var repo = new ProductTypeRepository(unit, _defaultVendorID);

        var types = unit.Service<Product>().GetAll(c => _vendorIDs.Any(x => x == c.SourceVendorID) && c.IsConfigurable)
          .Select(c => c.VendorItemNumber)
          .ToList()
          .Select(c => c.Split(new char[] { '-' }).Try(l => l[1], string.Empty))
          .Distinct()
          .ToList();

        var filledIn = repo.GetAll().Where(c => c.Type != null);

        return List((from p in types
                     where !string.IsNullOrEmpty(p)
                     let existing = filledIn.FirstOrDefault(c => c.Type == p)
                     select new ProductTypeViewModel()
                     {
                       Type = p,
                      // Translation = existing == null ? string.Empty : (string.IsNullOrEmpty(existing.Translation) ? string.Empty : existing.Translation),
                       IsBra = existing == null ? false : existing.IsBra,
                       ProductType = existing == null ? string.Empty : (existing.ProductType == null ? string.Empty : existing.ProductType.ToString())
                     }).AsQueryable());


      }
    }

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult Create(ProductTypeModel model)
    {
      try
      {

        using (var unit = GetUnitOfWork())
        {
          var repo = new ProductTypeRepository(unit, _defaultVendorID);
          repo.Add(model);
        }

        return Success("Successfully added type");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong ", e);
      }
    }

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult Update(string _Type, bool? IsBra, string ProductType)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var repo = new ProductTypeRepository(unit, _defaultVendorID);
          repo.Update(_Type, null, IsBra, ProductType, null);
        }

        return Success("Successfully added type");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong ", e);
      }
    }

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult Delete(string id)
    {
      try
      {

        using (var unit = GetUnitOfWork())
        {
          var repo = new ProductTypeRepository(unit, _defaultVendorID);
          repo.Delete(id);
        }

        return Success("Successfully added type");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong ", e);
      }
    }

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult GetTranslations(string type)
    {
      using (var unit = GetUnitOfWork())
      {
        var repo = new ProductTypeRepository(unit, _defaultVendorID);

        var types = unit.Service<Product>().GetAll(c => _vendorIDs.Any(x => x == c.SourceVendorID) && c.IsConfigurable).Select(c => c.VendorItemNumber).ToList().Select(c => c.Split(new char[] { '-' }).Try(l => l[1], string.Empty)).Distinct().ToList();//.Substring(5, 3)).ToList();
        var filledIn = repo.GetAll().Where(c => c.Type != null);

        var existingType = types.FirstOrDefault(x => x == type);
        if (existingType == null && !string.IsNullOrEmpty(type))
          return Failure("Type does not exist or type is null or empty");

        var record = filledIn.FirstOrDefault(x => x.Type == existingType);
        if (record == null)
          record = new ProductTypeModel();

        if (record.Translations == null)
          record.Translations = new Dictionary<int, string>();

        return List(u => (from l in unit.Service<Language>().GetAll().ToList()
                          join p in record.Translations on l.LanguageID equals p.Key into temp
                          from tr in temp.DefaultIfEmpty()
                          select new
                          {
                            l.LanguageID,
                            Language = l.Name,
                            Name = tr.Value,
                            @Type = type
                          }).AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult SetTranslation(int _LanguageID, string type, string name)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var repo = new ProductTypeRepository(unit, _defaultVendorID);
          var record = repo.GetAll().FirstOrDefault(x => x.Type == type);

          if (record == null)
          {
            repo.Add(new ProductTypeModel
            {
              Type = type
            });
          }
          repo.Update(type, name, record.IsBra, (record != null && record.ProductType == null ? string.Empty : record.ProductType.ToString()), _LanguageID);

          unit.Save();
          return Success("Update translation successfully");
        }
      }
      catch (Exception e)
      {
        return Failure("Something went wrong: ", e);
      }
    }
  }
}
