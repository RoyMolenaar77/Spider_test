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
  public class ProductTypeTranslation : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Product type translator"; }
    }

    protected override void Process()
    {
      var vendorIDsToProcessSetting = GetConfiguration().AppSettings.Settings["VendorIDs"];
      if(vendorIDsToProcessSetting == null)
        throw new Exception("Setting VendorIDs is missing");

      var vendorIDsToProcess = vendorIDsToProcessSetting.Value.Split(',').Select(x => Convert.ToInt32(x)).ToArray();

      var defaultVendorIDSetting = GetConfiguration().AppSettings.Settings["VendorID"];
      if(defaultVendorIDSetting == null)
        throw new Exception("Setting VendorID is missing");

      var defaultVendorID = Int32.Parse(defaultVendorIDSetting.Value);

      using (var unit = GetUnitOfWork())
      {
        ProductTypeRepository repositoryProductTypes = new ProductTypeRepository((IServiceUnitOfWork)unit, defaultVendorID);
        var att = unit.Scope.Repository<ProductAttributeMetaData>().GetAll().ToList().FirstOrDefault(c => c.AttributeCode.ToLower() == "type");
        var typeAttributeID = att.AttributeID;
        var productAttributeValueRepo = unit.Scope.Repository<ProductAttributeValue>();
        var products = unit.Scope.Repository<Product>().GetAll(c => vendorIDsToProcess.Contains(c.SourceVendorID) && c.IsConfigurable).ToList();

        foreach (var product in products)
        {
          var typeCode = product.VendorItemNumber.Try(c => c.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[1], string.Empty);


          if (string.IsNullOrEmpty(typeCode)) //if model code can't be retrieved from the product, continue
            continue;

          var typeTranslations = repositoryProductTypes.Get(typeCode);

          if (typeTranslations == null || typeTranslations.Translations == null)//if translation hasn't been set, continue
            continue;

          foreach (var typeTranslation in typeTranslations.Translations)
          {
            var languageID = typeTranslation.Key;
            var translation = typeTranslation.Value;

            var value = product.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == typeAttributeID && c.LanguageID == languageID);
            if (value == null)
            {
              value = new ProductAttributeValue()
              {
                Product = product,
                AttributeID = typeAttributeID,
                LanguageID = languageID
              };

              productAttributeValueRepo.Add(value);
            }

            value.Value = translation;
          }

          unit.Save();
        }
      }
    }
  }
}
