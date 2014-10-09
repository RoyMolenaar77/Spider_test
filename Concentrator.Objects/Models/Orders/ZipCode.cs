using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Orders
{
  public class ZipCode
  {
    public Int32 ZipCodeID { get; set; }

    public String PCWIJK { get; set; }

    public String PCLETTER { get; set; }

    public String PCREEKSID { get; set; }

    public Int32 PCREEKSVAN { get; set; }

    public Int32 PCREEKSTOT { get; set; }

    public String PCCITYTPG { get; set; }

    public String PCCITYNEN { get; set; }

    public String PCSTRTPG { get; set; }

    public String PCSTRNEN { get; set; }

    public String PCSTROFF { get; set; }

    public String PCCITYEXT { get; set; }

    public String PCSTREXT { get; set; }

    public Int32 PCGEMEENTEID { get; set; }

    public String PCGEMEENTENAAM { get; set; }

    public String PCPROVINCIE { get; set; }

    public Int32 PCCEBUCO { get; set; }

  }
}