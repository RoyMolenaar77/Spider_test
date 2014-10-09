using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Services.DTO.Base;

namespace Concentrator.Objects.Services.DTO
{
  public class ProductDetailDto : DtoBase
  {
    public int ProductID { get; set; }

    public string ShortDescription { get; set; } //= product name

    public string BrandName { get; set; }

    public string LongDescription { get; set; }

    public int LanguageID { get; set; }

    public string ModelName { get; set; }

    public string ProductName { get; set; }

    public string ExtraDescription { get; set; }

    public string Barcode { get; set; }

    public string ProductGroup { get; set; }

    public List<int> SequenceList { get; set; }

    public string CustomItemNumber { get; set; }

    public List<ImagesForSingleProductDto> ImageList { get; set; } //can be deleted

    public List<AttributeNameAndValueDto> AttributeNameValueList { get; set; }

  }


  public class AttributeNameAndValueDto
  {
    public string AttributeName { get; set; }

    public string AttributeValue { get; set; }

    public int AttributeValueID { get; set; }

    public string ImagePath { get; set; }
  }

  //whole class can be deleted
  public class ImagesForSingleProductDto
  {
    public string MediaPath { get; set; }

    public int Sequence { get; set; }

  }


  
}
