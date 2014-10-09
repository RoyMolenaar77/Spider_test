using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Web.Shared.Models
{
  public class ContentFilter
  {
    public int? minImageSize { get; set; }
    public int? maxImageSize { get; set; }
    public bool? hasImage { get; set; }
    public int? LanguageID { get; set; }
    public bool? hasLongContentDescription { get; set; }

    public bool? Media { get; set; }
    public bool? Images { get; set; }
    public bool? MediaAndSpecs { get; set; }
    public bool? Specs { get; set; }
    public bool? Video { get; set; }

    public bool? UnmappedBrandVendor { get; set; }
    public bool? MappedBrandVendor { get; set; }

    public Int32? VendorIdentification { get; set; }
    public string RemainderIdentification { get; set; }

    public bool? VendorOverlay { get; set; }
    public bool? VendorBase { get; set; }
    public bool? VendorRemainder { get; set; }

    public bool? ActivePgvIdentification { get; set; }
    public bool? UnactivePgvIdentification { get; set; }

    public bool? MissingActiveProducts { get; set; }
    public bool? ActiveProducts { get; set; }

    public bool? MatchedProduct { get; set; }
    public bool? UnmatchedProduct { get; set; }

    public bool? MissingPrice { get; set; }
    public bool? ActivePrice { get; set; }

    public bool? PreferredContent { get; set; }
    public bool? UnpreferredContent { get; set; }

    public bool? ProcessedOrder { get; set; }
    public bool? UnprocessedOrder { get; set; }

    public bool? PublishedProduct { get; set; }
    public bool? UnpublishedProduct { get; set; }

    public bool? ErrorOrder { get; set; }
    public bool? NonErrorOrder { get; set; }

    public int? Status { get; set; }
    public int? ResponseType { get; set; }

    public bool? SentToWehkamp { get; set; }
    public bool? ReadyForWehkamp { get; set; }
    public bool? SentToWehkampAsDummy { get; set; }

    public bool? BtF { get; set; }


  }
}