using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Concentrator.Vendors.PFA
{
  public class RecordTypeCache
  {


    static RecordTypeCache()
    {
      PopulateTypeCache(Assembly.GetExecutingAssembly(), BaseNameSpace);
    }

    private static string BaseNameSpace = "Concentrator.Vendors.PFA.Helpers";

    private static Dictionary<string, Type> _typeCache;
    public static Dictionary<string, Type> TypeCache
    {
      get
      {
        //if (_typeCache == null)


        return _typeCache;
      }
    }

    private static Type[] _typeList;
    public static Type[] AllTypes
    {
      get
      {
        return _typeList;
      }
    }

    private static void PopulateTypeCache(Assembly assembly, string nameSpace)
    {
      _typeList = assembly.GetTypes().Where(t => !t.IsGenericType && t.Namespace.StartsWith(nameSpace)).ToArray();
      _typeCache = _typeList.ToDictionary(x => x.Name, y => y);

    }
  }
}
