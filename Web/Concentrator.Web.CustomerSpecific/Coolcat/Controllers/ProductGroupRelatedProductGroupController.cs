using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.CustomerSpecific.Coolcat.Models;
using Concentrator.Web.CustomerSpecific.Coolcat.Repositories;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.Web.CustomerSpecific.Coolcat.Controllers
{
  public class ProductGroupRelatedProductGroupController : BaseController
  {
    private const int VendorID = 1;
    private const string SettingKey = "ProductGroupRelatedProductGroups";

    public ProductGroupRelatedProductGroupController()
    {
      using (var unit = GetUnitOfWork())
      {
        var sett = unit.Service<VendorSetting>().Get(c => c.VendorID == VendorID && c.SettingKey == SettingKey);
        if (sett == null)
        {
          sett = new VendorSetting()
          {
            VendorID = VendorID,
            SettingKey = SettingKey,
            Value = (new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Relations"))).ToString()
          };
          unit.Service<VendorSetting>().Create(sett);
          unit.Save();
        }

      }
    }

    [RequiresAuthentication(Functionalities.GetProductGroupRelatedProductGroup)]
    public ActionResult GetAll()
    {

      using (var unit = GetUnitOfWork())
      {
        var document = XDocument.Parse(unit.Service<VendorSetting>().Get(c => c.VendorID == VendorID && c.SettingKey == SettingKey).Value);
        var repo = new ProductGroupRelatedProductGroupsRepository(document);

        var results = repo.GetAllRules().AsQueryable();

        return List(results.AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.GetProductGroupRelatedProductGroup)]
    public IQueryable<ProductGroupRelatedProductGroupsModel> GetAllProductGroupRelatedProductGroups()
    {

      using (var unit = GetUnitOfWork())
      {
        var document = XDocument.Parse(unit.Service<VendorSetting>().Get(c => c.VendorID == VendorID && c.SettingKey == SettingKey).Value);
        var repo = new ProductGroupRelatedProductGroupsRepository(document);

        var results = repo.GetAllRules().AsQueryable();

        return results.AsQueryable();
      }
    }


   
    [RequiresAuthentication(Functionalities.CreateProductGroupRelatedProductGroup)]
    public ActionResult Create(ProductGroupRelatedProductGroupsModel model)
    {

      using (var unit = GetUnitOfWork())
      {
        try
        {
          var setting = unit.Service<VendorSetting>().Get(c => c.VendorID == VendorID && c.SettingKey == SettingKey);
          var document = XDocument.Parse(setting.Value);
          var repo = new ProductGroupRelatedProductGroupsRepository(document);

          repo.Add(model);
          setting.Value = document.ToString();

          unit.Save();
          return Success("Successfully added relation");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong: ", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductGroupRelatedProductGroup)]
    public ActionResult Update(ProductGroupRelatedProductGroupsModel model, string _ProductGroup)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var setting = unit.Service<VendorSetting>().Get(c => c.VendorID == VendorID && c.SettingKey == SettingKey);
          var document = XDocument.Parse(setting.Value);
          var repo = new ProductGroupRelatedProductGroupsRepository(document);

          model.ProductGroup = _ProductGroup;
          repo.Update(model);
          setting.Value = document.ToString();
          unit.Save();
          return Success("Successfully modified relation");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong: ", e);
        }
      }
    }

    [RequiresAuthentication(Functionalities.DeleteProductGroupRelatedProductGroup)]
    public ActionResult Delete(ProductGroupRelatedProductGroupsModel model, string _ProductGroup)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var setting = unit.Service<VendorSetting>().Get(c => c.VendorID == VendorID && c.SettingKey == SettingKey);
          var document = XDocument.Parse(setting.Value);
          var repo = new ProductGroupRelatedProductGroupsRepository(document);
          model.ProductGroup = _ProductGroup;
          repo.Delete(model);
          setting.Value = document.ToString();
          unit.Save();
          return Success("Successfully deleted relation");
        }
        catch (Exception e)
        {
          return Failure("Something went wrong: ", e);
        }
      }
    }

    //[RequiresAuthentication(Functionalities.GetProductGroupRelatedProductGroup)]
    //public ActionResult GetList()
    //{
    //  return List(unit =>
    //              from s in unit.Service<ProductGroupRelatedProductGroup>().GetAll()
    //              select new
    //              {
    //                s.ID,
    //                s.ProductGroup,
    //                s.RelatedProductGroups
    //              });
    //}

    //[RequiresAuthentication(Functionalities.CreateProductGroupRelatedProductGroup)]
    //public ActionResult Create()
    //{

    //  return Create<ProductGroupRelatedProductGroup>();
    //}

    //[RequiresAuthentication(Functionalities.DeleteProductGroupRelatedProductGroup)]
    //public ActionResult Delete(string productGroup)
    //{
    //  return Delete<ProductGroupRelatedProductGroup>(x => x.ProductGroup == productGroup);
    //}

    //[RequiresAuthentication(Functionalities.UpdateProductGroupRelatedProductGroup)]
    //public ActionResult Update(string productGroup)
    //{
    //  return Update<ProductGroupRelatedProductGroup>(x => x.ProductGroup == productGroup);
    //}

  }
}