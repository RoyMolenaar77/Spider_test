using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.Objects.Web.Models
{
    public class ManagementPortalFilter
    {
      public const string SessionKey = "ManagementPortalFilter";
      /// <summary>
      /// BeforeDate Filter  
      /// </summary>
      public DateTime? FromDate { get; set; }


      /// <summary>
      /// AfterDate filter  
      /// </summary>
      public DateTime? UntilDate { get; set; }

      /// <summary>
      /// AfterDate filter  
      /// </summary>
      public DateTime? OnDate { get; set; }

    }
}
