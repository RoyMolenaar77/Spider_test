using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Concentrator.Core.Services.Models.Media
{
  public class Media
  {
    public MediaTypes Type { get; set; }

    [XmlIgnore()]
    public Uri Url { get; set; }

    [JsonIgnore]
    [XmlElement("Url")]
    public string _Url
    {
      get { return Url.ToString(); }
      set { Url = new Uri(value); }
    }

    public int Sequence { get; set; }
  }
}
