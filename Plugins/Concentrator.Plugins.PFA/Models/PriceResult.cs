using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Models
{
  public class PriceResult
  {
    public string art_code { get; set; }

    public string color_code { get; set; }

    public string size_code { get; set; }

    public DateTime? start_date { get; set; }

    public DateTime? end_date { get; set; }

    public string discount_code { get; set; }

    public string currency_code { get; set; }

    public decimal price { get; set; }
    
    public string country_code { get; set; }
  }
}
