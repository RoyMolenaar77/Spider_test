using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.CustomerSpecific.Sapph.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Sapph
{
  public class ModelTranslationProcessor : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Sapph translate model descriptions"; }
    }

    protected override void Process()
    {
      var vendorIDsToProcessSetting = GetConfiguration().AppSettings.Settings["VendorIDs"];
      if (vendorIDsToProcessSetting == null)
        throw new Exception("Setting VendorIDs is missing");

      var vendorIDsToProcess = vendorIDsToProcessSetting.Value.Split(',').Select(x => Convert.ToInt32(x)).ToArray();

      var defaultVendorIDSetting = GetConfiguration().AppSettings.Settings["VendorID"];
      if (defaultVendorIDSetting == null)
        throw new Exception("Setting VendorID is missing");

      var defaultVendorID = Int32.Parse(defaultVendorIDSetting.Value);

      using (var unit = GetUnitOfWork())
      {
        ProductModelRepository repositoryModels = new ProductModelRepository((IServiceUnitOfWork)unit, defaultVendorID);

        var modelAttributeID = unit.Scope.Repository<ProductAttributeMetaData>().GetAll().ToList().FirstOrDefault(c => c.AttributeCode.ToLower() == "model").AttributeID;
        var productAttributeValueRepo = unit.Scope.Repository<ProductAttributeValue>();
        var products = unit.Scope.Repository<Product>().GetAll(c => vendorIDsToProcess.Contains(c.SourceVendorID) && c.IsConfigurable).ToList();

        foreach (var product in products)
        {
          var modelCode = product.VendorItemNumber.Try(c => c.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0], string.Empty);

          if (string.IsNullOrEmpty(modelCode)) //if model code can't be retrieved from the product, continue
            continue;

          var modelTranlsation = repositoryModels.Get(modelCode);
          if (modelTranlsation == null || string.IsNullOrEmpty(modelTranlsation.Translation))//if translation hasn't been set, continue
            continue;

          var value = product.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == modelAttributeID);
          if (value == null)
          {
            value = new ProductAttributeValue()
            {
              Product = product,
              AttributeID = modelAttributeID
            };

            productAttributeValueRepo.Add(value);
          }

          value.Value = modelTranlsation.Translation;
        }

        unit.Save();
      }
    }
  }
}
