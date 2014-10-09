using System;
using System.Linq;

namespace Concentrator.Tasks.Models
{
  public class PricatEnvelop : PricatLineBase
  {
    public String Receiver;
    public String Sender;
    public String Reference;
    public Boolean TestIndication;

    public static PricatEnvelop Parse(String line)
    {
      var result = new PricatEnvelop();
      var values = SplitRegex
        .Split(line)
        .Select(value => value.Trim('\"'))
        .ToArray();

      if (values.Length != 5)
      {
        return null;
      }

      result.Sender = values[1];
      result.Receiver = values[2];
      result.Reference = values[3];
      result.TestIndication = values[4] == "1";

      return result;
    }

    private PricatEnvelop()
    {
    }
  }
}