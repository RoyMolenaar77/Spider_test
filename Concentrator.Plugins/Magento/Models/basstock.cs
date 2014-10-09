using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Magento.Models
{
  public class basstock
  {
    public string sku { get; set; }
    public int bas_store_id { get; set; }
    public int qty { get; set; }
  }

  public class basstore
  {
    public int store_id { get; set; }
    public string companyno { get; set; }
  }
}
