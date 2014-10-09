#region Usings

using System;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Vendors.Bulk;

#endregion

namespace Concentrator.Tasks.Euretco.RSO.EDI.Importers
{
  public partial class ProductContentImporter
  {
    private sealed class ProductTypeProcessor : ProductContentRecordProcessor
    {
      private const String Header = "ProductType";

      public ProductTypeProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public ProductTypeProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'Collectie' could not be loaded.");
        }
      }

      [ProductAttribute(attributeCode: "ProductType", isConfigurable: true)]
      private ProductAttributeMetaData ProductTypeAttribute { get; set; }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = ProductTypeAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[Header, ProductTypeAttribute.DefaultValue]
        });
      }
    }
  }
}