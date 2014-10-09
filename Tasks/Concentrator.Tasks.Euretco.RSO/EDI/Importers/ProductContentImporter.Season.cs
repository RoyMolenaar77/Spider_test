using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Euretco.RSO.EDI.Importers
{
  using Concentrator.Objects.Models.Vendors;
  using Objects.Models.Attributes;
  using Objects.Vendors.Bulk;

  public partial class ProductContentImporter
  {
    private sealed class SeasonProcessor : ProductContentRecordProcessor
    {
      private const String HeaderSeason = "Seizoen";

      [ProductAttribute(attributeCode: "Season", isConfigurable: true)]
      private ProductAttributeMetaData SeasonAttribute
      {
        get;
        set;
      }

      public SeasonProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public SeasonProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'Seizoen' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = SeasonAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderSeason, SeasonAttribute.DefaultValue]
        });
      }
    }
  }
}
