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
    private sealed class DeliveryTimeProcessor : ProductContentRecordProcessor
    {
      private const String HeaderDeliveryTime = "DeliveryTime";

      [ProductAttribute(attributeCode: "Delivery_Time", isConfigurable: true)]
      private ProductAttributeMetaData DeliveryTimeAttribute
      {
        get;
        set;
      }

      public DeliveryTimeProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public DeliveryTimeProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'DeliveryTime' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = DeliveryTimeAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderDeliveryTime, DeliveryTimeAttribute.DefaultValue]
        });
      }
    }
  }
}
