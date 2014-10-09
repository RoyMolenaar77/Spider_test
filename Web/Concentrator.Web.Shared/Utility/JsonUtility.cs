using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Concentrator.Web.Shared.Utility
{
  public static class JsonUtility
  {
    public static string SearilizeParams(object Param)
    {
      JavaScriptSerializer serializer = new JavaScriptSerializer();

      return serializer.Serialize(Param);

    }
  }
}
