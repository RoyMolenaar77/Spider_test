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
    private sealed class CollectionProcessor : ProductContentRecordProcessor
    {
      private const String HeaderCollection = "Collectie";

      [ProductAttribute(attributeCode: "Collection", isConfigurable: true)]
      private ProductAttributeMetaData CollectionAttribute
      {
        get;
        set;
      }

      public CollectionProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public CollectionProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'Collectie' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = CollectionAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderCollection, CollectionAttribute.DefaultValue]
        });
      }
    }
  }
}
