using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.ui.Management.Models
{
  public class FactSheetModel
  {
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public string BrandName { get; set; }
    public List<DescriptionModel> DescriptionModels { get; set; }
    public List<AttributeModel> AttributeModels { get; set; }
    public List<ImageModel> ImageModels { get; set; }
    public string BarCode { get; set; }
    public decimal? Price { get; set; }
    public string logoPath { get; set; }
  }


}

