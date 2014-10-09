using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Concentrator.Tasks.Euretco.Rso.BizTalk.ProductExport.Models
{
  /// <summary>
  /// This class contains a product output model, including child classes, used for XML-serialization towards BizTalk
  /// </summary>
  [Serializable]
  [XmlRoot("product")]
  public class ProductBizTalkModel
  {
    private const string DateFormat = "yyyy-MM-dd";
    private readonly NumberFormatInfo DecimalPointFormatProvider = new NumberFormatInfo { NumberDecimalSeparator = "." };

    [XmlElement("sku")]
    public string Sku { set; get; }

    [XmlElement("attribute_set")]
    public string AttributeSet { set; get; }

    [XmlArray("websites")]
    [XmlArrayItem("website")]
    public List<string> Websites { set; get; }

    [XmlIgnore]
    public string Name { get; set; }
    [XmlElement("name")]
    public XmlCDataSection FormattedName {
      set { Name = value.Value; }
      get { return new XmlDocument().CreateCDataSection(Name); }
    }

    [XmlIgnore]
    public string Description { get; set; }
    [XmlElement("description")]
    public XmlCDataSection FormattedDescription{
      set { Description = value.Value; }
      get { return new XmlDocument().CreateCDataSection(Description); }
    }

    [XmlIgnore]
    public string ShortDescription { get; set; }
    [XmlElement("short_description")]
    public XmlCDataSection FormattedShortDescription {
      set { ShortDescription = value.Value; }
      get { return new XmlDocument().CreateCDataSection(ShortDescription); }
    }

    [XmlIgnore]
    public string ShortSummary { get; set; }
    [XmlElement("meta_title")]
    public XmlCDataSection FormattedShortSummary
    {
      set { ShortSummary = value.Value; }
      get { return new XmlDocument().CreateCDataSection(ShortSummary); }
    }

    [XmlIgnore]
    public string LongSummary { get; set; }
    [XmlElement("meta_description")]
    public XmlCDataSection FormattedLongSummary
    {
      set { LongSummary = value.Value; }
      get { return new XmlDocument().CreateCDataSection(LongSummary); }
    }

    [XmlIgnore]
    public Double Price { set; get; }
    [XmlElement("price")]
    public String FormattedPrice {
      set { Price = Double.Parse(value, DecimalPointFormatProvider); }
      get { return Price.ToString(DecimalPointFormatProvider); }
    }

    [XmlElement("taxClass")]
    public string TaxClass { set; get; }

    //[XmlIgnore]
    //public Double SpecialPrice { set; get; }
    //[XmlElement("special_price")]
    //public String FormattedSpecialPrice {
    //  set { SpecialPrice = Double.Parse(value, DecimalPointFormatProvider); }
    //  get { return SpecialPrice.ToString(DecimalPointFormatProvider); }
    //}

    //[XmlIgnore]
    //public DateTime SpecialPriceStartDate { set; get; }
    //[XmlElement("special_from_date")]
    //public string FormattedSpecialPriceStartDate {
    //  set { SpecialPriceStartDate = DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture); }
    //  get { return SpecialPriceStartDate.ToString(DateFormat); }
    //}

    //[XmlIgnore]
    //public DateTime SpecialPriceEndDate { set; get; }
    //[XmlElement("special_to_date")]
    //public string FormattedSpecialPriceEndDate {
    //  set { SpecialPriceEndDate = DateTime.ParseExact(value, DateFormat, CultureInfo.InvariantCulture); }
    //  get { return SpecialPriceEndDate.ToString(DateFormat); }
    //}

    [XmlElement("weight")]
    public string Weight { set; get; }

    [XmlElement("status")]
    public int Status { set; get; }

    [XmlElement("visibility")]
    public int Visibility { set; get; }

    [XmlArray("attributes")]
    [XmlArrayItem("attribute", typeof(Attribute))]
    public List<Attribute> Attributes { set; get; }

    [XmlArray("variants")]
    [XmlArrayItem("variant", typeof(Variant))]
    public List<Variant> Variants { set; get; }

    public ProductBizTalkModel()
    {
      Websites = new List<string>();
      //SpecialPriceStartDate = DateTime.MinValue;
      //SpecialPriceEndDate = DateTime.MaxValue;
      Attributes = new List<Attribute>();
      Variants = new List<Variant>();
    }
  }

  [DebuggerDisplay("AttributeCode = {Code}")]
  public class Attribute
  {
    [XmlElement("code")]
    public string Code { set; get; }

    //Suppress node when null
    [XmlArray("text_value"), DefaultValue(null)]
    [XmlArrayItem("value", typeof(string))]
    public List<string> TextValue { set; get; }

    //Suppress node when null
    [XmlArray("multi_value"), DefaultValue(null)]
    [XmlArrayItem("option", typeof(string))]
    public List<string> MultiValue { set; get; }

    public Attribute()
    {
      TextValue = null;
      MultiValue = null;
    }
  }

  public class Variant
  {
    [XmlElement("sku")]
    public string Sku { set; get; }

    [XmlElement("status")]
    public int Status { set; get; }

    [XmlElement("price_correction")]
    public int PriceCorrection { set; get; }

    [XmlElement("visibility")]
    public int Visibility { set; get; }

    [XmlArray("options")]
    [XmlArrayItem("option", typeof(Option))]
    public List<Option> Options { set; get; }

    public Variant()
    {
      Options = new List<Option>();
    }
  }

  public class Option
  {
    [XmlElement("code")]
    public string Code { set; get; }

    [XmlElement("value")]
    public string Value { set; get; }
  }
}