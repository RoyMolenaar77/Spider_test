#region Usings

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace Concentrator.Tasks.Euretco.RSO.Navision.Models
{
  [Serializable]
  [XmlRoot("ARTICLE_IMPORT")]
  public class XmlDocumentModel
  {
    public XmlDocumentModel()
    {
      Articles = new List<Article>();
    }

    [XmlElement("ARTICLE_INFO")]
    public List<Article> Articles { set; get; }
  }

  [Serializable]
  public class Article
  {
    [XmlElement("ARTICLE_NO")]
    public string ArticleNumber { set; get; }

    [XmlElement("ARTICLE_DESCRIPTION")]
    public string ArticleDescription { set; get; }

    [XmlElement("ARTICLE_DESCRIPTION2")]
    public string ArticleDescription2 { set; get; }

    [XmlElement("SUPPLIER_NO")]
    public string SupplierNumber { set; get; }

    [XmlElement("SUPPLIER_ITEM_NO")]
    public string SupplierItemNumber { set; get; }

    [XmlElement("REQUISITION_METHOD")]
    public string RequisitionMethod { set; get; }

    [XmlElement("UNIT_GROSS_WEIGHT")]
    public string UnitGrossWeight { set; get; }

    [XmlElement("UNIT_NETTO_WEIGHT")]
    public string UnitNettoWeight { set; get; }

    [XmlElement("TARIFF_NO")]
    public string TariffNo { set; get; }

    [XmlElement("COUNTRY_PURCHASED")]
    public string CountryPurchased { set; get; }

    [XmlElement("COUNTRY_ORIGIN")]
    public string CountryOrigin { set; get; }

    [XmlElement("ITEM_CATEGORY")]
    public string ItemCategory { set; get; }

    [XmlElement("PRODUCT_CODE")]
    public string ProductCode { set; get; }

    [XmlElement("ITEM_STATUS")]
    public string ItemStatus { set; get; }

    [XmlElement("BRAND")]
    public string Brand { set; get; }

    [XmlElement("COLLECTION")]
    public string Collection { set; get; }

    [XmlElement("SEASON")]
    public string Season { set; get; }

    [XmlElement("THEME")]
    public string Theme { set; get; }

    [XmlElement("QUALITY")]
    public string Quality { set; get; }

    [XmlElement("DELIVERY_PERIOD")]
    public string DeliveryPeriod { set; get; }

    [XmlElement("PURCHASING_CODE")]
    public string PurchasingCode { set; get; }

    [XmlElement("BUYING_PRICE")]
    public string BuyingPrice { set; get; }

    [XmlElement("SALES_PRICE")]
    public string SalesPrice { set; get; }

    [XmlElement("VARIANT_INFO")]
    public List<VariantModel> Variants { set; get; }

    public Article()
    {
      Variants = new List<VariantModel>();
    }
  }

  [Serializable]
  public class VariantModel
  {
    [XmlElement("PIM_ARTICLE_NO")]
    public string PimArticleNumber { set; get; }

    [XmlElement("VARIANT_DESCRIPTION")]
    public string VariantDescription { set; get; }

    [XmlElement("BARCODE_INFO")]
    public BarcodeInfoModel BarcodeInfo { set; get; }

    [XmlElement("COLOR_INFO")]
    public ColorInfoModel ColorInfo { set; get; }

    [XmlElement("SIZE_INFO")]
    public SizeInfoModel SizeInfo { set; get; }

    public VariantModel()
    {
      BarcodeInfo = new BarcodeInfoModel();
      ColorInfo = new ColorInfoModel();
      SizeInfo = new SizeInfoModel();
    }
  }

  public class BarcodeInfoModel
  {
    [XmlElement("EANCODE")]
    public string EanCode { set; get; }
  
  }

  public class ColorInfoModel
  {
    [XmlElement("COLOR")]
    public string Color { set; get; }

    [XmlElement("COLOR_DESCRIPTION")]
    public string ColorDescription { set; get; }

    [XmlElement("COLOR_SORTING")]
    public string ColorSorting { set; get; }
  }

  public class SizeInfoModel
  {
    [XmlElement("SIZE")]
    public string Size { set; get; }

    [XmlElement("SIZE_DESCRIPTION")]
    public string SizeDescription { set; get; }

    [XmlElement("SIZE_SORTING")]
    public string SizeSorting { set; get; }
  }
}