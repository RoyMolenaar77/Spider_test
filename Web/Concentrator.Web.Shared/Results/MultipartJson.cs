using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Concentrator.Web.Shared.Results
{
  public class MultipartJson : JsonResult
  {
    private object _data;

    public MultipartJson(object data)
    {
      _data = data;
    }


    public override void ExecuteResult(ControllerContext context)
    {
      ContentType = "text/html";
      Data = _data;

      base.ExecuteResult(context);
    }
  }
}
