﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.WebToPrint
{
  public class WebToPrintComposite : BaseModel<WebToPrintComposite>
  {
    public int CompositeID { get; set; }
    public int ProjectID { get; set; }
    public string Name { get; set; }
    public string Data { get; set; }
    public virtual WebToPrintProject WebToPrintProject { get; set; }

    public override System.Linq.Expressions.Expression<Func<WebToPrintComposite, bool>> GetFilter()
    {
      return null;
    }
  }
}
