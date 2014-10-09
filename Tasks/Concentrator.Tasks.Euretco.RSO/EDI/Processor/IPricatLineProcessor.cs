using System;
using System.Collections.Generic;
using System.Diagnostics;

using Concentrator.Objects.Models.Vendors;
using Concentrator.Tasks.Euretco.Rso.EDI.Models;
using Concentrator.Tasks.Models;
using Concentrator.Objects.Vendors.Bulk;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Localization;

namespace Concentrator.Tasks.Euretco.Rso.EDI.Processor
{
  public interface IPricatLineProcessor
  {
    IEnumerable<VendorAssortmentBulk.VendorAssortmentItem> ProcessPricatGroupedLines(
      IUnitOfWork unit, 
      TraceSource traceSource,
      Vendor vendor, 
      string vendorItemNumber, 
      Language defaultLanguage,
      IEnumerable<PricatLine> pricatLines, 
      PricatProductAttributeStore productAttributes, 
      IDictionary<String, String> pricatColorMapping, 
      IEnumerable<PricatSize> pricatSizes);

    String TranslateSize(IEnumerable<PricatSize> pricatSizesLookup, String brand, String model, String group, String size);
  }
}