using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Concentrator.Web.Shared.Binders
{
  public class FlagEnumBinder : DefaultModelBinder
  {
    public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
    {
      var model = bindingContext.Model;
      var modelType = bindingContext.ModelType;

      //if enum we are translating to has attribute [Flags]
      //convert value to proper type   
      if (bindingContext.ModelType.IsEnum && bindingContext.ModelType.IsDefined(typeof(FlagsAttribute), false))
      {
        var value = (from c in controllerContext.RequestContext.HttpContext.Request[bindingContext.ModelName].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                     select int.Parse(c)).ToArray().Sum();

        return value;
      }
      else //use default binding
      {
        return base.BindModel(controllerContext, bindingContext);
      }

    }
  }
}