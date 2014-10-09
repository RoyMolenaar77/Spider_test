using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Concentrator.Configuration;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Web;
using Concentrator.Objects.Web.Models;
using Concentrator.ui.Management.Models;
using Concentrator.ui.Management.Models.Anychart;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Ninject.Planning.Targets;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductDescriptionController : BaseController
  {
    private const String AttributeNotSpecified = "-1";

    #region properties
    public int MaterialAttributeID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["MaterialAttributeID"] ?? AttributeNotSpecified);
      }
    }

    public int MaterialDescriptionAttributeID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["MaterialDescriptionAttributeID"] ?? AttributeNotSpecified);
      }
    }

    public int KraagvormAttributeID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["KraagvormAttributeID"] ?? AttributeNotSpecified);
      }
    }

    public int DessinAttributeID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["DessinAttributeID"] ?? AttributeNotSpecified);
      }
    }

    public int PijpwijdteAttributeID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["PijpwijdteAttributeID"] ?? AttributeNotSpecified);
      }
    }

    public int ReadyForWehkampAttributeID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["ReadyForWehkampAttributeID"] ?? AttributeNotSpecified);
      }
    }

    private const String VendorNotSpecified = "-1";

    public int ConcentratorVendorID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["ConcentratorVendorID"] ?? VendorNotSpecified);
      }
    }

    public int WehkampCCVendorID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["WehkampCCVendorID"] ?? VendorNotSpecified);
      }
    }

    public int WehkampATVendorID
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings["WehkampATVendorID"] ?? VendorNotSpecified);
      }
    }

    #endregion

    private const string ConcentratorSectionGroupName = "concentrator";
    private const string DefaultJsFieldDataType = "textfield";

    private static class DescriptionCodes
    {
      public const String ProductName = "ProductName";
      public const String LongContentDescription = "LongDescription";
      public const String ShortContentDescription = "ShortDescription";
      public const String LongSummaryDescription = "LongSummaryDescription";
      public const String ShortSummaryDescription = "ShortSummaryDescription";
    }

    private IEnumerable<GeneralField> GetProductDetailsByProduct(int productID, int languageID, int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = unit
          .Service<Product>()
          .GetAll(c => c.ProductID == productID)
          .FirstOrDefault();

        if (product != null)
        {

          yield return new GeneralField
          {
            Label = "Product Name",
            Name = DescriptionCodes.ProductName,
            Type = "textfield"
          };

          yield return new GeneralField
          {
            Label = "Short Description",
            Name = DescriptionCodes.ShortContentDescription,
            Type = "textfield"
          };

          yield return new GeneralField
          {
            Label = "Long Description",
            Name = DescriptionCodes.LongContentDescription,
            Type = "textarea"
          };

          if (ConcentratorSection.Default.Management.ProductBrowser.General.DisplaySummaryFields)
          {
            yield return new GeneralField
            {
              Label = "Short Summary Description",
              Name = DescriptionCodes.ShortSummaryDescription,
              Type = "textfield"
            };

            yield return new GeneralField
            {
              Label = "Long Summary Description",
              Name = DescriptionCodes.LongSummaryDescription,
              Type = "textarea"
            };
          }

          var attributes = GetAttributesByConfiguration().ToArray();

          foreach (var attribute in attributes)
          {
            yield return attribute;
          }
        }
      }
    }

    [RequiresAuthentication(Functionalities.GetProductDescription)]
    public ActionResult GetProductAttributesByProduct(int productID, int languageID, int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var result = GetProductDetailsByProduct(productID, languageID, vendorID).ToArray();

        return Json(new
        {
          data = result,
          success = true
        });
      }
    }

    private IEnumerable<GeneralField> GetAttributesByConfiguration()
    {
      using (var unit = GetUnitOfWork())
      {
        var attributeCodes = ConcentratorSection.Default.Management.ProductBrowser.General
          .Cast<GeneralAttributeElement>()
          .Select(x => new GeneralField
            {
              Label = x.DisplayName,
              Name = x.Code,
              Type = !String.IsNullOrEmpty(x.Type)
                ? x.Type
                : DefaultJsFieldDataType,
              HiddenName = x.Code,
              DefaultValue = x.DefaultValue
            });
        return attributeCodes;
      }
    }

    [RequiresAuthentication(Functionalities.GetProductDescription)]
    public ActionResult GetByProduct(int productID, int languageID, int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = unit.Service<Product>().Get(c => c.ProductID == productID);

        var vendorsByName = unit
          .Service<Vendor>()
          .GetAll()
          .ToDictionary(vendor => vendor.Name, vendor => vendor.VendorID);

        var description = unit.Service<ProductDescription>().GetAll(c => c.ProductID == productID && c.LanguageID == languageID && c.VendorID == vendorID).FirstOrDefault();

        var productAttributeValues = product.ProductAttributeValues
          .Where(attribute => !attribute.LanguageID.HasValue || attribute.LanguageID == languageID);

        var model = new Dictionary<string, string>();

        foreach (var configuredAttribute in GetAttributesByConfiguration())
        {
          var attributeMetaData = unit.Service<ProductAttributeMetaData>().Get(x => x.AttributeCode == configuredAttribute.Name &&
            (ConcentratorSection.Default.Management.ProductBrowser.General.CreateAttributeByVendor ? x.VendorID == vendorID : true));

          if (attributeMetaData != null)
          {
            var attributeValue = productAttributeValues.FirstOrDefault(x => x.AttributeID == attributeMetaData.AttributeID);
            if (attributeValue != null)
              model.Add(configuredAttribute.Name, attributeValue.Value);
            else
              if (!string.IsNullOrEmpty(configuredAttribute.DefaultValue))
              {
                model.Add(configuredAttribute.Name, configuredAttribute.DefaultValue);
              }
          }
        }

        if (description != null)
        {
          model.Add(DescriptionCodes.ShortContentDescription, description.ShortContentDescription);
          model.Add(DescriptionCodes.LongContentDescription, description.LongContentDescription);
          model.Add(DescriptionCodes.ProductName, description.ProductName);
          model.Add(DescriptionCodes.ShortSummaryDescription, description.ShortSummaryDescription);
          model.Add(DescriptionCodes.LongSummaryDescription, description.LongSummaryDescription);
        }

        return Json(new
        {
          success = true,
          data = model
        });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductDescription)]
    [ValidateInput(false)]
    public ActionResult Update(int productID, int languageID, int vendorID, bool isSearched, string materialDescription, int? materialOptionID, int? dessinOptionID, int? kraagvormOptionID, int? pijpwijdteOptionID, bool propagate = true)
    {

      using (var unit = GetUnitOfWork())
      {
        if (materialDescription == null)
          materialDescription = "";

        ProductDescription pd = new ProductDescription();
        TryUpdateModel(pd);

        try
        {
          var product = unit.Service<Product>().Get(c => c.ProductID == productID);

          ((IProductService)unit.Service<Product>()).SetOrUpdateProductDescription(product, true, pd.LongContentDescription, pd.PDFSize, pd.PDFUrl, pd.ProductName, pd.Quality, pd.ShortContentDescription, pd.Url, pd.VendorID, pd.LanguageID);

          ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeOption(product, MaterialAttributeID, materialOptionID, true);
          ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeOption(product, DessinAttributeID, dessinOptionID, true);
          ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeOption(product, KraagvormAttributeID, kraagvormOptionID, true);
          ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeOption(product, PijpwijdteAttributeID, pijpwijdteOptionID, true);

          ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeValue(product, MaterialDescriptionAttributeID, materialDescription, true);

          var vendorWehkampAT = unit.Service<VendorAssortment>().Get(x => x.ProductID == productID && x.VendorID == WehkampATVendorID);
          var vendorWehkampCC = unit.Service<VendorAssortment>().Get(x => x.ProductID == productID && x.VendorID == WehkampCCVendorID);

          if (vendorWehkampAT != null || vendorWehkampCC != null)
          {
            var isReadyForWehkamp = CalcaluteProductReadyForWehkamp(product, pd, unit, vendorWehkampAT);

            if (isReadyForWehkamp) //if all mandatory fields are filled in then update readyforwehkamp
              ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeValue(product, ReadyForWehkampAttributeID, "True", true);
            else
              ((IProductService)unit.Service<Product>()).SetOrUpdateAttributeValue(product, ReadyForWehkampAttributeID, "False", true);
          }

          unit.Save();
          return Success("Successfully updated product description");
        }
        catch (Exception e)
        {
          return Failure(e.Message);
        }
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetAvailableContentVendorsForProduct()
    {
      const int vendorType = (Int32)VendorType.Content;

      using (var unit = GetUnitOfWork())
      {
        var contentVendors = unit.Scope
          .Repository<Vendor>()
          .GetAll(vendor => (vendor.VendorType & vendorType) > 0 && vendor.IsActive)
          .Select(vendor => new
          {
            VendorID = vendor.VendorID,
            VendorName = vendor.Name
          })
          .ToArray();

        return List(contentVendors.AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.GetProduct)]
    public ActionResult GetExistingContentVendorsForProduct(int productID)
    {
      const int vendorType = (Int32)VendorType.Content;

      using (var unit = GetUnitOfWork())
      {
        var contentVendors = unit.Scope
          .Repository<ProductDescription>()
          .Include(x => x.Vendor)
          .GetAll(x => (x.Vendor.VendorType & vendorType) > 0 && x.ProductID == productID)
          .Select(x => new
          {
            VendorID = x.VendorID,
            VendorName = x.Vendor.Name
          })
          .ToList();

        return List(contentVendors.AsQueryable());
      }
    }

    [RequiresAuthentication(Functionalities.UpdateProductDescription)]
    [ValidateInput(false)]
    public ActionResult UpdateByAttribute(int productID, int languageID, int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = unit.Service<Product>().Get(p => p.ProductID == productID);
        var productService = unit.Service<Product>() as IProductService;
        var productAttributesStructure = GetAttributesByConfiguration();


        //.Where(attribute => attribute.VendorID == vendorID)
        //.ToLookup(attribute => attribute.AttributeCode);
        var productDescription = product.ProductDescriptions.SingleOrDefault(x => x.VendorID == vendorID && x.LanguageID == languageID);

        if (productDescription == null)
        {
          product.ProductDescriptions.Add(productDescription = new ProductDescription { VendorID = vendorID, LanguageID = languageID });
        }

        try
        {
          if (productService != null)
          {
            foreach (var name in Request.Params.Keys.OfType<String>())
            {
              switch (name)
              {
                case DescriptionCodes.ProductName:
                  productDescription.ProductName = Request.Params[name];
                  break;

                case DescriptionCodes.LongContentDescription:
                  productDescription.LongContentDescription = Request.Params[name];
                  break;

                case DescriptionCodes.LongSummaryDescription:
                  productDescription.LongSummaryDescription = Request.Params[name];
                  break;

                case DescriptionCodes.ShortContentDescription:
                  productDescription.ShortContentDescription = Request.Params[name];
                  break;

                case DescriptionCodes.ShortSummaryDescription:
                  productDescription.ShortSummaryDescription = Request.Params[name];
                  break;
              }

              if (productAttributesStructure.Any(x => x.Name == name))
              {
                var attribute = GetOrCreateProductAttributeMetaData(name, vendorID, unit);

                productService.SetOrUpdateAttributeValue(product, attribute.AttributeID, Request.Params[name], true, languageID);

              }
            }
          }

          unit.Save();

          return Success("Successfully updated product description");
        }
        catch (Exception exception)
        {
          return Failure(exception.Message);
        }
      }
    }

    private ProductAttributeMetaData GetOrCreateProductAttributeMetaData(string attributeCode, int vendorID, IServiceUnitOfWork unit)
    {
      ProductAttributeMetaData attribute = null;

      var attributes = unit.Service<ProductAttributeMetaData>().GetAll(x => x.AttributeCode == attributeCode);
      if (attributes.Any())
      {
        //get attribute by vendor, create if not exists
        if (ConcentratorSection.Default.Management.ProductBrowser.General.CreateAttributeByVendor)
        {
          attribute = attributes.FirstOrDefault(x => x.VendorID == vendorID);
        }
        else
        {
          attribute = attributes.FirstOrDefault();
        }
      }
      else
      {
        throw new Exception(string.Format("Attribute '{0}' does not exist, please create first.", attributeCode));
      }

      if (attribute == null)
      {
        //create and return
        var baseAttribute = attributes.FirstOrDefault();
        attribute = new ProductAttributeMetaData
        {
          AttributeCode = baseAttribute.AttributeCode,
          ProductAttributeGroupID = baseAttribute.ProductAttributeGroupID,
          FormatString = baseAttribute.FormatString,
          DataType = baseAttribute.DataType,
          Index = baseAttribute.Index,
          IsVisible = baseAttribute.IsVisible,
          NeedsUpdate = baseAttribute.NeedsUpdate,
          VendorID = vendorID,
          IsSearchable = baseAttribute.IsSearchable,
          Sign = baseAttribute.Sign,
          CreatedBy = baseAttribute.CreatedBy,
          CreationTime = DateTime.Now,
          Mandatory = baseAttribute.Mandatory,
          DefaultValue = baseAttribute.DefaultValue,
          IsConfigurable = baseAttribute.IsConfigurable,
          ConfigurablePosition = baseAttribute.ConfigurablePosition,
          HasOption = baseAttribute.HasOption,
          IsSlider = baseAttribute.IsSlider,
          ProductAttributeNames = new List<ProductAttributeName>()
        };

        unit.Service<ProductAttributeMetaData>().Create(attribute);

        foreach (var attributeName in baseAttribute.ProductAttributeNames)
        {
          attribute.ProductAttributeNames.Add(new ProductAttributeName
          {
            LanguageID = attributeName.LanguageID,
            Name = attributeName.Name
          });
        }

        unit.Save();
      }

      return attribute;
    }

    private bool CalcaluteProductReadyForWehkamp(Product p, ProductDescription productDescription, IServiceUnitOfWork unit, VendorAssortment vendorAT)
    {
      //check for product descriptions
      if (!ReadyforWehkampProductDescription(productDescription, p.ProductID, unit))
        return false;

      //check for materialdescription attribute
      if (!ReadyForWehkampAttributes(p, MaterialDescriptionAttributeID))
        return false;

      //check for attribute option material
      if (!ReadyForWehkampAttributes(p, MaterialAttributeID))
        return false;

      if (vendorAT != null)
      {
        //check (only for AT) attribute option dessin
        if (!ReadyForWehkampAttributes(p, DessinAttributeID))
          return false;
      }

      return true;
    }

    private bool ReadyforWehkampProductDescription(ProductDescription productDescription, int productID, IServiceUnitOfWork unit)
    {
      if (productDescription.VendorID != ConcentratorVendorID)
        productDescription = unit.Service<ProductDescription>().Get(x => x.VendorID == ConcentratorVendorID && x.ProductID == productID);

      if (productDescription != null && !string.IsNullOrEmpty(productDescription.LongContentDescription) && !string.IsNullOrEmpty(productDescription.ShortContentDescription))
        return true;
      else
        return false;
    }

    private bool ReadyForWehkampAttributes(Product p, int attributeID)
    {
      var attributeValue = p.ProductAttributeValues.FirstOrDefault(x => x.AttributeID == attributeID && !string.IsNullOrEmpty(x.Value));

      if (attributeValue != null)
        return true;
      else
        return false;
    }

    [RequiresAuthentication(Functionalities.UpdateProductDescription)]
    public ActionResult Clone(int productID, int sourceVendorID, int targetVendorID)
    {
      if (sourceVendorID == targetVendorID)
      {
        return Failure("Source- and target vendor cannot be the same");
      }

      try
      {
        using (var unit = GetUnitOfWork())
        {
          var product = unit.Scope
            .Repository<Product>()
            .Include(p => p.ProductDescriptions)
            .GetSingle(p => p.ProductID == productID);

          if (product != null)
          {
            var sourceDescriptions = product.ProductDescriptions.Where(pd => pd.VendorID == sourceVendorID).ToArray();
            var targetDescriptions = product.ProductDescriptions.Where(pd => pd.VendorID == targetVendorID).ToDictionary(pd => pd.LanguageID);

            foreach (var sourceDescription in sourceDescriptions)
            {
              var targetDescription = default(ProductDescription);

              if (!targetDescriptions.TryGetValue(sourceDescription.LanguageID, out targetDescription))
              {
                product.ProductDescriptions.Add(targetDescription = new ProductDescription
                {
                  LanguageID = sourceDescription.LanguageID,
                  VendorID = targetVendorID
                });
              }

              targetDescription.ProductName = sourceDescription.ProductName;
              targetDescription.LongContentDescription = sourceDescription.LongContentDescription;
              targetDescription.ShortContentDescription = sourceDescription.ShortContentDescription;
              targetDescription.ShortSummaryDescription = sourceDescription.ShortSummaryDescription;
              targetDescription.LongSummaryDescription = sourceDescription.LongSummaryDescription;
            }

            unit.Save();
          }
        }
      }
      catch (Exception ex)
      {
        return Failure(String.Format("Cloning product descriptions failed: {0}", ex.Message));
      }

      return Success("Successfully cloned product descriptions.");
    }

    [RequiresAuthentication(Functionalities.GetProductDescription)]
    public ActionResult GetMissingLongDescriptionsCount(ContentPortalFilter filter)
    {
      MergeSession(filter, ContentPortalFilter.SessionKey);

      try
      {
        int? vendorid = null;

        if (ConfigurationManager.AppSettings["CheckForDescriptionVendorID"] != null)
          vendorid = int.Parse(ConfigurationManager.AppSettings["CheckForDescriptionVendorID"]);


        using (var unit = GetUnitOfWork())
        {
          var callbackUI = "content-item";
          var points = (from langCount in ((IProductService)unit.Service<Product>()).GetMissingLongDescriptionsCount(filter.Connectors, filter.Vendors, filter.BeforeDate, filter.AfterDate, filter.OnDate, filter.IsActive, filter.ProductGroups, filter.Brands, filter.LowerStockCount, filter.GreaterStockCount, filter.EqualStockCount, filter.Statuses, descriptionVendorID: vendorid)
                        select new PieChartPoint(langCount.Key.Name, langCount.Value, action: new AnychartAction(callbackUI, new
                        {
                          LanguageID = langCount.Key.LanguageID,
                          hasLongContentDescription = false
                        }))
                    ).ToList();

          Serie serie = new Serie(new List<Point>(points), "Missing content", "Default");

          return View("Anychart/DefaultPieChart", new AnychartComponentModel(new List<Serie>() { serie }));
        }
      }
      catch (Exception ex)
      {
        return Failure("Unable to retrieve the missing content", ex);
      }
    }

    [RequiresAuthentication(Functionalities.DeleteProductDescription)]
    public ActionResult DeleteVendorContent(int productID, int vendorID)
    {
      using (var unit = GetUnitOfWork())
      {
        String message = String.Empty;

        var content = (unit.Service<ProductDescription>().Get(x => x.ProductID == productID && x.VendorID == vendorID));

        if (content == null)
        {
          message = "Content doesn't exsist for this vendor";
          return Json(new
          {
            Success = false,
            Message = message
          });
        }

        unit.Service<ProductDescription>().Delete(content);
        message = "Content successfully deleted for this vendor";


        return Json(new
        {
          Success = true,
          Message = message
        });
      }
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult CopyProductData(int productID, int selectedProductID, string Description, string Attributes)
    {
      using (var unit = GetUnitOfWork())
      {
        var baseProduct = unit.Service<Product>().Get(x => x.ProductID == productID);
        var overrideProduct = unit.Service<Product>().Get(x => x.ProductID == selectedProductID);

        //General
        if (Description == "on")
        {
          baseProduct.ProductDescriptions.ForEach((desc, idx) =>
          {
            var odesc = overrideProduct.ProductDescriptions.FirstOrDefault(x => x.LanguageID == desc.LanguageID && x.VendorID == desc.VendorID);

            if (odesc == null)
            {
              odesc = new ProductDescription()
              {
                LanguageID = desc.LanguageID,
                ProductID = overrideProduct.ProductID,
                VendorID = desc.VendorID
              };

              unit.Service<ProductDescription>().Create(odesc);
            }

            odesc.LongContentDescription = desc.LongContentDescription;
            odesc.LongSummaryDescription = desc.LongSummaryDescription;
            odesc.ModelName = desc.ModelName;
            odesc.PDFSize = desc.PDFSize;
            odesc.PDFUrl = desc.PDFUrl;
            odesc.ProductName = desc.ProductName;
            odesc.Quality = desc.Quality;
            odesc.ShortContentDescription = desc.ShortContentDescription;
            odesc.ShortSummaryDescription = desc.ShortSummaryDescription;
            odesc.Url = desc.Url;
            odesc.WarrantyInfo = desc.WarrantyInfo;
          });
        }

        // Specs
        if (Attributes == "on")
        {
          baseProduct.ProductAttributeValues.ForEach((att, idx) =>
          {
            var oatt = overrideProduct.ProductAttributeValues.FirstOrDefault(x => x.AttributeID == att.AttributeID);

            if (oatt == null)
            {
              oatt = new Objects.Models.Attributes.ProductAttributeValue()
              {
                AttributeID = att.AttributeID,
                LanguageID = att.LanguageID,
                ProductID = overrideProduct.ProductID,
                AttributeValueGroupID = att.AttributeValueGroupID
              };
              unit.Service<ProductAttributeValue>().Create(oatt);
            }
            oatt.Value = att.Value;
          });
        }

        unit.Save();

        return Json(new
        {
          success = true,
          message = "Successfully copied the data"
        });
      }


    }
  }
}