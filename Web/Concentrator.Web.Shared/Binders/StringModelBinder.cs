using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.Web.Shared.Binders
{
  public class StringModelBinder : IModelBinder
  {
    #region IModelBinder Members

    public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      bindingContext.ThrowArgNull("bindingContext");

      var stringValue = controllerContext.RequestContext.HttpContext.Request[bindingContext.ModelName];

      if (string.IsNullOrEmpty(stringValue)) stringValue = null;

      return stringValue;
    }

    #endregion
  }
}