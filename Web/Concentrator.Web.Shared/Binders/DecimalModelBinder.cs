using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace Concentrator.Web.Shared.Binders
{
  public class DecimalModelBinder : IModelBinder
  {
    #region IModelBinder Members

    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      bindingContext.ThrowArgNull("bindingContext");

      var value = controllerContext.RequestContext.HttpContext.Request[bindingContext.ModelName];

      if (string.IsNullOrEmpty(value)) return null;

      var d = Decimal.Parse(value,CultureInfo.InvariantCulture);

      return d;
    }

    #endregion
  }
}