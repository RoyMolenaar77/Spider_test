using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.Web.Shared.Binders
{
  public class ArrayModelBinder<TItem> : DefaultModelBinder
  {
    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      var model = bindingContext.Model;
      var modelType = bindingContext.ModelType;

      if ((model == null) && modelType.IsArray)
      {
        string value = controllerContext.HttpContext.Request[bindingContext.ModelName];
        if (String.IsNullOrWhiteSpace(value))
          return new TItem[0];

      }
      return base.BindModel(controllerContext, bindingContext);
    }
  }

}