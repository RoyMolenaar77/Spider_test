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
    private sealed class SportProcessor : ProductContentRecordProcessor
    {
      private const String HeaderSport = "Sport";

      [ProductAttribute(attributeCode: "Sport", isConfigurable: true)]
      private ProductAttributeMetaData SportAttribute
      {
        get;
        set;
      }

      public SportProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public SportProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'Sport' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = SportAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderSport, SportAttribute.DefaultValue]
        });
      }
    }
  }
}
