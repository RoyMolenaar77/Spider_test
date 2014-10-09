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
    private sealed class AreaProcessor : ProductContentRecordProcessor
    {
      private const String HeaderArea = "Terrein";

      [ProductAttribute(attributeCode: "Area", isConfigurable: true)]
      private ProductAttributeMetaData AreaAttribute
      {
        get;
        set;
      }

      public AreaProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public AreaProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'Terrein' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = AreaAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderArea, AreaAttribute.DefaultValue]
        });
      }
    }
  }
}
