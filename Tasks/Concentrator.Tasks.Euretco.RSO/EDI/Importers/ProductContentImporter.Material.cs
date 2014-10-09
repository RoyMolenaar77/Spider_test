using System;

namespace Concentrator.Tasks.Euretco.RSO.EDI.Importers
{
  using Objects.Models.Vendors;
  using Objects.Models.Attributes;
  using Objects.Vendors.Bulk;

  public partial class ProductContentImporter
  {
    private sealed class MaterialProcessor : ProductContentRecordProcessor
    {
      private const String HeaderMaterial = "Materiaal";

      [ProductAttribute(attributeCode: "Material", isConfigurable: true)]
      private ProductAttributeMetaData MaterialAttribute
      {
        get;
        set;
      }

      public MaterialProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public MaterialProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'Material' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = MaterialAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderMaterial, MaterialAttribute.DefaultValue]
        });
      }
    }
  }
}
