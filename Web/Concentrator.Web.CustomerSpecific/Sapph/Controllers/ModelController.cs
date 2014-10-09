using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.CustomerSpecific.Sapph.Models;
using Concentrator.Web.CustomerSpecific.Sapph.Repositories;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Concentrator.Web.CustomerSpecific.Sapph.Controllers
{
  public class ModelController : BaseController
  {
    private readonly int[] _vendorIDs = { 50, 51 };
    private int _defaultVendorID = 50;

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult GetAll()
    {
      using (var unit = GetUnitOfWork())
      {
        var repo = new ProductModelRepository(unit, _defaultVendorID);

        var types = unit.Service<Product>().GetAll(c => _vendorIDs.Any(x=> x == c.SourceVendorID) && c.IsConfigurable)
          .Select(c => c.VendorItemNumber)
          .ToList()
          .Select(c => c.Split(new char[] { '-' }).Try(l => l[0], string.Empty))
          .Distinct()
          .ToList();

        var filledIn = repo.GetAll();

        return List((from p in types
                     where !string.IsNullOrEmpty(p)
                     let existing = filledIn.FirstOrDefault(c => c.ModelCode == p)
                     select new ModelDescriptionModel()
                     {
                       ModelCode = p,
                       Translation = existing == null ? string.Empty : existing.Translation
                     }).AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.Sapph)]
    public ActionResult Create(ModelDescriptionModel model)
    {
      try
      {

        using (var unit = GetUnitOfWork())
        {
          var repo = new ProductModelRepository(unit, _defaultVendorID);
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
    public ActionResult Update(string _ModelCode, string Translation)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          var repo = new ProductModelRepository(unit, _defaultVendorID);
          repo.Update(_ModelCode, Translation);
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
          var repo = new ProductModelRepository(unit, _defaultVendorID);
          repo.Delete(id);
        }

        return Success("Successfully removed type");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong ", e);
      }
    }
  }
}