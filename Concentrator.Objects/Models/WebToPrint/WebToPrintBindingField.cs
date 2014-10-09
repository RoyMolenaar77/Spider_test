using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.WebToPrint
{
  public enum BindingFieldType
  {
    Unknown = 0,
    Int = 2,
    String = 4,
    Double = 6,
    ImageURL = 8,
    Other = 10,
    Static = 12
  }

  public enum BindingFieldOptions
  {
      IndexValue = 1,
      StaticImageSearch = 2,
      StaticTextInput = 4,
      ProductIDSearch = 8,
      ProductCustomerIDSearch = 16
  }

  public class WebToPrintBindingField : BaseModel<WebToPrintBindingField>
  {
    public int FieldID { get; set; }
    public int BindingID { get; set; }
    public string Name { get; set; }
    public byte Type { get; set; }
    public int Options { get; set; }
    public int SearchType { get; set; }

    public WebToPrintBinding WebToPrintBinding { get; set; }

    public bool IsInput()
    {
      return (Type % 2) == 0;
    }

    public BindingFieldType GetBindingType()
    {
      return (BindingFieldType)(Type - (Type % 2));
    }

    public List<int> GetActiveOptions()
    {
        List<int> eList = new List<int>();

        foreach (var e in Enum.GetValues(typeof(BindingFieldOptions)))
        {
            if ((Options & (int)e) == (int)e)
                eList.Add((int)e);
        }

        return eList;
    }

    public override System.Linq.Expressions.Expression<Func<WebToPrintBindingField, bool>> GetFilter()
    {
      return null;
    }
  }
}
