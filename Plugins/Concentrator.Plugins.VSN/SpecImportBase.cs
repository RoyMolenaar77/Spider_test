using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Data;
using Concentrator.Objects;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.VSN
{
  public abstract class SpecImportBase : VSNBase
  {
    protected void ProcessSpecTable(DataTable specTable, IUnitOfWork unit)
    {
      int generalAttributegroupID = GetGeneralAttributegroupID(unit);

      var specs = from r in specTable.Rows.Cast<DataRow>()
                  group r by new { ProductCode = r.Field<string>("ProductCode"), AttributeID = r.Field<long>("ProductPropertyTypeID"), AttributeName = r.Field<string>("ProductPropertyType") } into grp
                  let values = from v in grp
                               select v.Field<string>("ProductPropertyDescription")
                  select new
                           {
                             grp.Key.AttributeName,
                             grp.Key.ProductCode,
                             grp.Key.AttributeID,
                             Value = String.Join(", ", values.ToArray())
                           };

      var groupedByProduct = (from r in specs
                              group r by r.ProductCode
                                into grouped
                                select grouped);

      var attributeRepo = unit.Scope.Repository<ProductAttributeMetaData>();
      var attributeNameRepo = unit.Scope.Repository<ProductAttributeName>();

      int step = 100;
      int todo = groupedByProduct.Count();
      int done = 0;

      log.Info("Starting processing of specs. To process:  " + todo);
      //while (done < todo)
      //{
      //  var productsToProcess = groupedByProduct.Skip(done).Take(step);

      foreach (var row in groupedByProduct)
      {
        var productRow = row;
        log.DebugFormat("Processing product {0}", productRow.Key);
        var product = unit.Scope.Repository<VendorAssortment>().GetSingle(va => va.CustomItemNumber == productRow.Key && va.VendorID == VendorID && va.IsActive == true);
        if (product == null)
        {
          log.WarnFormat(
            "Cannot process specs for product with VSN number: {0} because it doesn't exist in Concentrator database",
            productRow.Key);
          continue;
        }




        foreach (var spec in productRow)
        {

          #region Attribute meta data

          var meta =
            attributeRepo.GetSingle(
              pam => pam.AttributeCode == spec.AttributeID.ToString() && pam.VendorID == VendorID);
          if (meta == null)
          {
            meta = new ProductAttributeMetaData
                     {
                       VendorID = VendorID,
                       AttributeCode = spec.AttributeID.ToString(),
                       IsVisible = true,
                       ProductAttributeGroupID = generalAttributegroupID,
                       Index = 0,
                       NeedsUpdate = true,
                       IsSearchable = false,
                       Sign = String.Empty
                     };
            attributeRepo.Add(meta);
          }

          #endregion Attribute meta data

          #region Attribute name
          if (meta.ProductAttributeNames == null) meta.ProductAttributeNames = new List<ProductAttributeName>();
          var attName =
            meta.ProductAttributeNames.FirstOrDefault(pal => pal.LanguageID == (int)LanguageTypes.Netherlands);
          if (attName == null)
          {
            attName = new ProductAttributeName
                        {
                          ProductAttributeMetaData = meta,
                          LanguageID = (int)LanguageTypes.Netherlands
                        };
            attributeNameRepo.Add(attName);
          }
          attName.Name = spec.AttributeName;

          #endregion


          #region Attribute value
          if (meta.ProductAttributeValues == null) meta.ProductAttributeValues = new List<ProductAttributeValue>();
          var attributeValue =
            meta.ProductAttributeValues.FirstOrDefault(
              pav => pav.ProductID == product.ProductID && pav.LanguageID == (int)LanguageTypes.Netherlands);
          if (attributeValue == null)
          {
            attributeValue = new ProductAttributeValue
                               {
                                 ProductID = product.ProductID,
                                 LanguageID = (int)LanguageTypes.Netherlands,
                                 ProductAttributeMetaData = meta
                               };
            unit.Scope.Repository<ProductAttributeValue>().Add(attributeValue);
          }
          attributeValue.Value = spec.Value.Cap(500);

          #endregion Attribute value



        }
        unit.Save();
      }

      //log.Info("Finished processing specs. Processed: " + done);
      //  }
      //  done += productsToProcess.Count();
      //}

    }
  }
}
