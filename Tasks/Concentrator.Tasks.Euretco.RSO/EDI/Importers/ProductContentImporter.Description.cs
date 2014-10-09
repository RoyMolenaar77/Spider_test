using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Euretco.RSO.EDI.Importers
{
  using Concentrator.Objects.Models.Vendors;
  using Objects.Vendors.Bulk;

  public partial class ProductContentImporter
  {
    private sealed class DescriptionProcessor : ProductContentRecordProcessor
    {
      private const String HeaderProductName = "Product Name";
      private const String HeaderContentShort = "Content (Short)";
      private const String HeaderContentLong = "Content (Long)";
      private const String HeaderSummaryShort = "Summary Short";
      private const String HeaderSummaryLong = "Summary Long";
      //private const String HeaderSummaryMeta = "Summary Meta";

      //[ProductAttribute(attributeCode: "meta_description", isConfigurable: true)] 
      //private const Int32 MetaDescriptionAttributeID = 0;

      //[ProductAttribute(attributeCode: "meta_keyword", isConfigurable: true)] 
      //private const Int32 MetaKeywordAttributeID = 0;

      //[ProductAttribute(attributeCode: "meta_title", isConfigurable: true)] 
      //private const Int32 MetaTitleAttributeID = 0;

      public DescriptionProcessor(Vendor vendor)
      {
        if (!ProductAttributeHelper.Bind(this, vendor))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public DescriptionProcessor()
      {
        if (!ProductAttributeHelper.Bind(this))
        {
          throw new Exception("One of the product attributes could not be loaded.");
        }
      }

      public override void Process(ProductContentRecord record)
      {
        VendorAssortment.VendorProductDescriptions.Add(new VendorAssortmentBulk.VendorProductDescription
        {
          CustomItemNumber = CustomItemNumber,
          DefaultVendorID = VendorID,
          LanguageID = LanguageID,
          VendorID = VendorID,
          ProductName = record[HeaderProductName, String.Empty],
          LongContentDescription = record[HeaderContentLong, String.Empty],
          LongSummaryDescription = record[HeaderSummaryLong, String.Empty],
          ShortContentDescription = record[HeaderContentShort, String.Empty],
          ShortSummaryDescription = record[HeaderSummaryShort, String.Empty]
        });

        //VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        //{
        //  AttributeID = MetaDescriptionAttributeID,
        //  CustomItemNumber = CustomItemNumber,
        //  DefaultVendorID = VendorID,
        //  LanguageID = LanguageID.ToString(),
        //  Value = record[HeaderSummaryLong, String.Empty]
        //});

        //VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        //{
        //  AttributeID = MetaKeywordAttributeID,
        //  CustomItemNumber = CustomItemNumber,
        //  DefaultVendorID = VendorID,
        //  LanguageID = LanguageID.ToString(),
        //  Value = record[HeaderSummaryMeta, String.Empty]
        //});

        //VendorAssortment.VendorImportAttributeValues.Add(new VendorAssortmentBulk.VendorImportAttributeValue
        //{
        //  AttributeID = MetaTitleAttributeID,
        //  CustomItemNumber = CustomItemNumber,
        //  DefaultVendorID = VendorID,
        //  LanguageID = LanguageID.ToString(),
        //  Value = record[HeaderSummaryShort, String.Empty]
        //});
      }
    }
  }
}
