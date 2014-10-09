using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Concentrator.ui.Management.Models
{
  public class CustomLabelModel
  {
    public int ProductGroupMappingID { get; set; }

    public int LanguageID { get; set; }

    public string Language { get; set; }

    public string CustomLabel { get; set; }

    public int ?  ConnectorID { get; set; }

    public string ConnectorName { get; set; }

  }
}