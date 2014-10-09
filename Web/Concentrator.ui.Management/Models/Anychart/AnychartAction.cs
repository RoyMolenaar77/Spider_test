using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Concentrator.ui.Management.Models.Anychart
{
  public class AnychartAction
  {


    /// <summary>
    /// The name of the JS function to execute
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// The type of the action. Defaults to a js action
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// The values of its parameters in the correct order
    /// </summary>
    public List<string> ArgumentValues { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arguments"></param>
    public AnychartAction(params object[] arguments)
    {
      Type = "call";
      Name = "Concentrator.WidgetCallbackHandler";
      ArgumentValues = SerializeValues(arguments);
    }


    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="arguments">Argument values</param>
    /// <param name="type">Type of action</param>
    /// <param name="_name">The name of the action to call</param>
    public AnychartAction(string _type = "call", string _name = "Concentrator.WidgetCallbackHandler", params object[] arguments)
    {
      Type = _type;
      Name = _name;
      ArgumentValues = SerializeValues(arguments);
    }

    private List<string> SerializeValues(object[] argumentValues)
    {
      JavaScriptSerializer serializer = new JavaScriptSerializer();
      List<string> values = new List<string>();

      foreach (var s in argumentValues)
      {
        var type = s.GetType();


        if (type == typeof(string) || type.IsValueType)
          values.Add(s.ToString());
        else
          values.Add(serializer.Serialize(s));

      }

      return values;
    }
  }
}