using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Concentrator.Core.Services.Models.Products
{
  public class Attribute
  {
    public int AttributeID { get; set; }

    public object Value { get; set; }

    [XmlIgnore()]
    public Uri ImageUrl { get; set; }

    [JsonIgnore]
    [XmlElement("ImageURL")]
    public string _ImageURL
    {
      get { return ImageUrl != null ? ImageUrl.ToString() : ""; }
      set { ImageUrl = new Uri(value); }
    }
  }
}
