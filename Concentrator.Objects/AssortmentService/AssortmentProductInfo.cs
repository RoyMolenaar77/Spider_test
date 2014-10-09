using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentProductInfo : AssortmentProductWithRelatedProducts
  {   
    public string LineType { get; set; }

    public DateTime? CutOffTime { get; set; }

    public int? DeliveryHours { get; set; }

    public AssortmentBrand Brand { get; set; }

    public List<AssortmentProductGroup> ProductGroups { get; set; }

    public List<AssortmentPrice> Prices { get; set; }

    public bool PresaleProduct { get; set; }

    public AssortmentStock Stock { get; set; }

    public List<AssortmentRetailStock> RetailStock { get; set; }

    public AssortmentContent Content { get; set; }

    public List<string> Barcodes { get; set; }    

    public List<AssortmentConfigurableAttribute> ConfigurableAttributes { get; set; }

    public AssortmentShopInformation ShopInfo { get; set; }

    public List<AssortmentProductImage> Images { get; set; }
  }
}
