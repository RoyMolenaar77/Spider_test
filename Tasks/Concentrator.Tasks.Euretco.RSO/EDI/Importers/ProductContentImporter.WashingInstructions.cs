using System;

namespace Concentrator.Tasks.Euretco.RSO.EDI.Importers
{
  using Objects.Models.Vendors;
  using Objects.Models.Attributes;
  using Objects.Vendors.Bulk;

  public partial class ProductContentImporter
  {
    private sealed class WashingInstructionsProcessor : ProductContentRecordProcessor
    {
      private const String HeaderWashingInstructions = "Wasvoorschrift";

      [ProductAttribute(attributeCode: "WashingInstructions", isConfigurable: true)]
      private ProductAttributeMetaData WashingInstructionsAttribute
      {
        get;
        set;
      }

      public WashingInstructionsProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public WashingInstructionsProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("The product attribute 'WashingInstructions' could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        {
          AttributeID = WashingInstructionsAttribute.AttributeID,
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          Value = record[HeaderWashingInstructions, WashingInstructionsAttribute.DefaultValue]
        });
      }
    }
  }
}
