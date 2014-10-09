using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;
//using Concentrator.Vendors.PFA.Helpers;

namespace Concentrator.Vendors.PFA.FileFormats
{
  public class FileFormatSelector
  {
    public static Type RecordSelector(MultiRecordEngine engine, string record)
    {

      //if (engine.LineNumber == 1)
      //  return typeof(EDI_DC40);

      try
      {
        string formatName = record.Substring(0, 30).Trim();

        Type t = null;
        RecordTypeCache.TypeCache.TryGetValue(formatName, out t);
        return t;

      }
      catch
      {
        return null;
      }
    }
  }
}
